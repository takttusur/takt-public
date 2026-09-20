using System.Text.Json;
using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Takt.Identity.API.Configuration;
using Takt.Identity.API.Constants;
using Takt.Identity.API.Persistence;
using Takt.Identity.API.Services;

namespace Takt.Identity.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/account")]
public sealed class AccountController : ControllerBase
{
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetMeAsync(
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] IdentityAppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var roles = await userManager.GetRolesAsync(user);
        var passkeyCount = await dbContext.Set<IdentityUserPasskey<long>>()
            .CountAsync(x => x.UserId.Equals(user.Id), cancellationToken);

        return Ok(new AccountMeResponse(
            user.Id,
            user.UserName ?? string.Empty,
            roles,
            user.TwoFactorEnabled,
            passkeyCount));
    }

    [HttpPost("password/change")]
    [Authorize]
    public async Task<IActionResult> ChangePasswordAsync(
        [FromBody] ChangePasswordRequest request,
        [FromServices] UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var result = await userManager.ChangePasswordAsync(user, request.CurrentPassword, request.NewPassword);
        if (!result.Succeeded)
        {
            return BadRequest(new ValidationProblemDetails(result.Errors.ToDictionary(x => x.Code, x => new[] { x.Description })));
        }

        await userManager.UpdateSecurityStampAsync(user);
        return NoContent();
    }

    [HttpPost("2fa/setup")]
    [AllowAnonymous]
    public async Task<IActionResult> SetupTwoFactorAsync(
        [FromBody] SetupTwoFactorRequest request,
        [FromServices] IAuthFlowStateService flowStateService,
        [FromServices] UserManager<ApplicationUser> userManager,
        CancellationToken cancellationToken)
    {
        var state = await flowStateService.GetActiveAsync(request.SetupToken, AuthFlowPurposes.TwoFactorSetup, cancellationToken);
        if (state?.UserId is null)
        {
            return Unauthorized();
        }

        var user = await userManager.FindByIdAsync(state.UserId.Value.ToString());
        if (user is null || user.IsDisabled)
        {
            return Unauthorized();
        }

        var key = await userManager.GetAuthenticatorKeyAsync(user);
        if (string.IsNullOrWhiteSpace(key))
        {
            await userManager.ResetAuthenticatorKeyAsync(user);
            key = await userManager.GetAuthenticatorKeyAsync(user);
        }

        var issuer = "Takt.Identity";
        var encodedIssuer = Uri.EscapeDataString(issuer);
        var encodedUser = Uri.EscapeDataString(user.UserName ?? string.Empty);
        var provisioningUri = $"otpauth://totp/{encodedIssuer}:{encodedUser}?secret={key}&issuer={encodedIssuer}&digits=6";

        return Ok(new TwoFactorSetupDataResponse(key!, provisioningUri));
    }

    [HttpPost("2fa/enable")]
    [AllowAnonymous]
    public async Task<IActionResult> EnableTwoFactorAsync(
        [FromBody] EnableTwoFactorRequest request,
        [FromServices] IAuthFlowStateService flowStateService,
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] ITokenService tokenService,
        CancellationToken cancellationToken)
    {
        var state = await flowStateService.GetActiveAsync(request.SetupToken, AuthFlowPurposes.TwoFactorSetup, cancellationToken);
        if (state?.UserId is null)
        {
            return Unauthorized();
        }

        var user = await userManager.FindByIdAsync(state.UserId.Value.ToString());
        if (user is null || user.IsDisabled)
        {
            return Unauthorized();
        }

        var isValid = await userManager.VerifyTwoFactorTokenAsync(
            user,
            Microsoft.AspNetCore.Identity.TokenOptions.DefaultAuthenticatorProvider,
            request.Code);
        if (!isValid)
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["code"] = ["Invalid authenticator code."]
            }));
        }

        user.TwoFactorEnabled = true;
        await userManager.UpdateAsync(user);
        await userManager.UpdateSecurityStampAsync(user);

        var consumed = await flowStateService.TryConsumeAsync(request.SetupToken, AuthFlowPurposes.TwoFactorSetup, cancellationToken);
        if (!consumed)
        {
            return Unauthorized();
        }

        var tokenPair = await tokenService.IssueAsync(user, cancellationToken);
        return Ok(new TokenResponse(
            tokenPair.AccessToken,
            tokenPair.AccessTokenExpiresAt,
            tokenPair.RefreshToken,
            tokenPair.RefreshTokenExpiresAt));
    }

    [HttpGet("2fa/status")]
    [Authorize]
    public async Task<IActionResult> TwoFactorStatusAsync([FromServices] UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.GetUserAsync(User);
        return user is null
            ? Unauthorized()
            : Ok(new TwoFactorStatusResponse(user.TwoFactorEnabled));
    }

    [HttpPost("passkeys/options")]
    [Authorize]
    public async Task<IActionResult> PasskeyCreationOptionsAsync(
        [FromBody] PasskeyOptionsRequest request,
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] IPasskeyHandler<ApplicationUser> passkeyHandler,
        [FromServices] IAuthFlowStateService flowStateService,
        [FromServices] IOptions<PasskeyOptions> passkeyOptionsAccessor,
        CancellationToken cancellationToken)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        if (!user.TwoFactorEnabled)
        {
            return Problem("2FA setup must be completed before passkey enrollment.", statusCode: StatusCodes.Status403Forbidden);
        }

        var userEntity = new PasskeyUserEntity
        {
            Id = user.Id.ToString(),
            Name = user.UserName ?? string.Empty,
            DisplayName = user.UserName ?? string.Empty
        };
        var result = await passkeyHandler.MakeCreationOptionsAsync(userEntity, HttpContext);
        if (string.IsNullOrWhiteSpace(result.AttestationState))
        {
            return Problem("Passkey attestation state was not created.", statusCode: StatusCodes.Status500InternalServerError);
        }

        var stateToken = await flowStateService.CreateAsync(
            AuthFlowPurposes.PasskeyRegistration,
            user.Id,
            new PasskeyRegistrationStatePayload(result.AttestationState, request.FriendlyName ?? "Unnamed device"),
            TimeSpan.FromMinutes(passkeyOptionsAccessor.Value.ChallengeTtlMinutes),
            cancellationToken);

        return Ok(new PasskeyCreationOptionsResponse(result.CreationOptionsJson, stateToken));
    }

    [HttpPost("passkeys/complete")]
    [Authorize]
    public async Task<IActionResult> CompletePasskeyRegistrationAsync(
        [FromBody] PasskeyCompleteRequest request,
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] IPasskeyHandler<ApplicationUser> passkeyHandler,
        [FromServices] IAuthFlowStateService flowStateService,
        CancellationToken cancellationToken)
    {
        var state = await flowStateService.GetActiveAsync(request.StateToken, AuthFlowPurposes.PasskeyRegistration, cancellationToken);
        if (state?.UserId is null)
        {
            return Unauthorized();
        }

        var currentUserIdRaw = userManager.GetUserId(User);
        var currentUserId = long.TryParse(currentUserIdRaw, out var parsedCurrentUserId)
            ? parsedCurrentUserId
            : (long?)null;
        if (currentUserId is null || currentUserId != state.UserId)
        {
            return Forbid();
        }

        var user = await userManager.FindByIdAsync(state.UserId.Value.ToString());
        if (user is null)
        {
            return Unauthorized();
        }

        var payload = JsonSerializer.Deserialize<PasskeyRegistrationStatePayload>(state.Payload);
        if (payload is null)
        {
            return Unauthorized();
        }

        var attestationResult = await passkeyHandler.PerformAttestationAsync(new PasskeyAttestationContext
        {
            HttpContext = HttpContext,
            CredentialJson = request.CredentialJson,
            AttestationState = payload.AttestationState
        });

        if (!attestationResult.Succeeded || attestationResult.Passkey is null)
        {
            return BadRequest(new { message = attestationResult.Failure?.Message ?? "Passkey attestation failed." });
        }

        var registeredPasskey = new UserPasskeyInfo(
            attestationResult.Passkey.CredentialId,
            attestationResult.Passkey.PublicKey,
            attestationResult.Passkey.CreatedAt,
            attestationResult.Passkey.SignCount,
            attestationResult.Passkey.Transports,
            attestationResult.Passkey.IsUserVerified,
            attestationResult.Passkey.IsBackupEligible,
            attestationResult.Passkey.IsBackedUp,
            attestationResult.Passkey.AttestationObject,
            attestationResult.Passkey.ClientDataJson)
        {
            Name = payload.FriendlyName
        };

        await userManager.AddOrUpdatePasskeyAsync(user, registeredPasskey);

        var consumed = await flowStateService.TryConsumeAsync(request.StateToken, AuthFlowPurposes.PasskeyRegistration, cancellationToken);
        if (!consumed)
        {
            return Conflict(new { message = "Passkey registration challenge has already been used." });
        }

        return NoContent();
    }

    [HttpGet("passkeys")]
    [Authorize]
    public async Task<IActionResult> GetPasskeysAsync([FromServices] UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var passkeys = await userManager.GetPasskeysAsync(user);
        var response = passkeys.Select(p => new PasskeyResponse(
            ToPasskeyId(p.CredentialId),
            p.Name ?? "Unnamed device",
            p.CreatedAt,
            p.IsUserVerified)).ToArray();

        return Ok(response);
    }

    [HttpPatch("passkeys/{id}")]
    [Authorize(Policy = Policies.StrongAuthenticationFresh)]
    public async Task<IActionResult> RenamePasskeyAsync(
        [FromRoute] string id,
        [FromBody] RenamePasskeyRequest request,
        [FromServices] UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var passkeyId = FromPasskeyId(id);
        var passkey = await userManager.GetPasskeyAsync(user, passkeyId);
        if (passkey is null)
        {
            return NotFound();
        }

        var updated = new UserPasskeyInfo(
            passkey.CredentialId,
            passkey.PublicKey,
            passkey.CreatedAt,
            passkey.SignCount,
            passkey.Transports,
            passkey.IsUserVerified,
            passkey.IsBackupEligible,
            passkey.IsBackedUp,
            passkey.AttestationObject,
            passkey.ClientDataJson)
        {
            Name = request.Name
        };

        await userManager.AddOrUpdatePasskeyAsync(user, updated);
        return NoContent();
    }

    [HttpDelete("passkeys/{id}")]
    [Authorize(Policy = Policies.StrongAuthenticationFresh)]
    public async Task<IActionResult> DeletePasskeyAsync(
        [FromRoute] string id,
        [FromServices] UserManager<ApplicationUser> userManager)
    {
        var user = await userManager.GetUserAsync(User);
        if (user is null)
        {
            return Unauthorized();
        }

        var passkeys = await userManager.GetPasskeysAsync(user);
        var credentialId = FromPasskeyId(id);
        var target = passkeys.SingleOrDefault(x => x.CredentialId.SequenceEqual(credentialId));
        if (target is null)
        {
            return NotFound();
        }

        var hasPassword = await userManager.HasPasswordAsync(user);
        if (!hasPassword && passkeys.Count == 1)
        {
            return Problem("Cannot remove the last usable authentication method.", statusCode: StatusCodes.Status409Conflict);
        }

        await userManager.RemovePasskeyAsync(user, credentialId);
        return NoContent();
    }

    private static string ToPasskeyId(byte[] credentialId) => Microsoft.IdentityModel.Tokens.Base64UrlEncoder.Encode(credentialId);
    private static byte[] FromPasskeyId(string id) => Microsoft.IdentityModel.Tokens.Base64UrlEncoder.DecodeBytes(id);

    private sealed record PasskeyRegistrationStatePayload(string AttestationState, string FriendlyName);
}

public sealed record AccountMeResponse(long Id, string UserName, IList<string> Roles, bool TwoFactorEnabled, int PasskeyCount);
public sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public sealed record SetupTwoFactorRequest(string SetupToken);
public sealed record TwoFactorSetupDataResponse(string SharedKey, string ProvisioningUri);
public sealed record EnableTwoFactorRequest(string SetupToken, string Code);
public sealed record TwoFactorStatusResponse(bool TwoFactorEnabled);
public sealed record PasskeyOptionsRequest(string? FriendlyName);
public sealed record PasskeyCreationOptionsResponse(string CreationOptionsJson, string StateToken);
public sealed record PasskeyCompleteRequest(string StateToken, string CredentialJson);
public sealed record PasskeyResponse(string Id, string Name, DateTimeOffset CreatedAt, bool IsUserVerified);
public sealed record RenamePasskeyRequest(string Name);
