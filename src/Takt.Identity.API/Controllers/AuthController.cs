using System.Text.Json;
using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using Takt.Identity.API.Configuration;
using Takt.Identity.API.Persistence;
using Takt.Identity.API.Services;

namespace Takt.Identity.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("password/login")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> PasswordLoginAsync(
        [FromBody] PasswordLoginRequest request,
        [FromServices] SignInManager<ApplicationUser> signInManager,
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] IAuthFlowStateService authFlowStateService,
        [FromServices] IOptions<SecurityOptions> securityOptionsAccessor,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByNameAsync(request.UserName);
        if (user is null || user.IsDisabled)
        {
            return Unauthorized();
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return Problem("Account is locked.", statusCode: StatusCodes.Status423Locked);
        }

        var passwordResult = await signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);
        if (!passwordResult.Succeeded)
        {
            if (passwordResult.IsLockedOut)
            {
                return Problem("Account is locked.", statusCode: StatusCodes.Status423Locked);
            }

            return Unauthorized();
        }

        var ttl = TimeSpan.FromMinutes(securityOptionsAccessor.Value.AuthenticationChallengeTtlMinutes);
        if (!user.TwoFactorEnabled)
        {
            var setupToken = await authFlowStateService.CreateAsync(
                AuthFlowPurposes.TwoFactorSetup,
                user.Id,
                new TwoFactorSetupPayload(),
                ttl,
                cancellationToken);

            return Ok(new TwoFactorSetupRequiredResponse("TwoFactorSetupRequired", setupToken));
        }

        var challengeToken = await authFlowStateService.CreateAsync(
            AuthFlowPurposes.TwoFactorVerify,
            user.Id,
            new TwoFactorChallengePayload(),
            ttl,
            cancellationToken);

        return Ok(new TwoFactorChallengeRequiredResponse("TwoFactorVerificationRequired", challengeToken));
    }

    [HttpPost("2fa/verify")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> VerifyTwoFactorAsync(
        [FromBody] TwoFactorVerifyRequest request,
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] IAuthFlowStateService authFlowStateService,
        [FromServices] ITokenService tokenService,
        CancellationToken cancellationToken)
    {
        var state = await authFlowStateService.GetActiveAsync(request.ChallengeToken, AuthFlowPurposes.TwoFactorVerify, cancellationToken);
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
            return Unauthorized();
        }

        var consumed = await authFlowStateService.TryConsumeAsync(
            request.ChallengeToken,
            AuthFlowPurposes.TwoFactorVerify,
            cancellationToken);

        if (!consumed)
        {
            return Unauthorized();
        }

        var tokenPair = await tokenService.IssueAsync(user, cancellationToken);
        return Ok(ToTokenResponse(tokenPair));
    }

    [HttpPost("passkey/options")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> CreatePasskeyRequestOptionsAsync(
        [FromBody] PasskeyAuthOptionsRequest request,
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] IPasskeyHandler<ApplicationUser> passkeyHandler,
        [FromServices] IAuthFlowStateService flowStateService,
        [FromServices] IOptions<PasskeyOptions> passkeyOptionsAccessor,
        CancellationToken cancellationToken)
    {
        ApplicationUser? user = null;
        if (!string.IsNullOrWhiteSpace(request.UserName))
        {
            user = await userManager.FindByNameAsync(request.UserName);
        }

        var result = await passkeyHandler.MakeRequestOptionsAsync(user!, HttpContext);
        if (string.IsNullOrWhiteSpace(result.AssertionState))
        {
            return Problem("Passkey assertion state was not created.", statusCode: StatusCodes.Status500InternalServerError);
        }

        var stateToken = await flowStateService.CreateAsync(
            AuthFlowPurposes.PasskeyAuthentication,
            user?.Id,
            new PasskeyAssertionStatePayload(result.AssertionState),
            TimeSpan.FromMinutes(passkeyOptionsAccessor.Value.ChallengeTtlMinutes),
            cancellationToken);

        return Ok(new PasskeyRequestOptionsResponse(result.RequestOptionsJson, stateToken));
    }

    [HttpPost("passkey/complete")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> CompletePasskeySignInAsync(
        [FromBody] PasskeyAuthCompleteRequest request,
        [FromServices] IPasskeyHandler<ApplicationUser> passkeyHandler,
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] IAuthFlowStateService flowStateService,
        [FromServices] ITokenService tokenService,
        CancellationToken cancellationToken)
    {
        var state = await flowStateService.GetActiveAsync(request.StateToken, AuthFlowPurposes.PasskeyAuthentication, cancellationToken);
        if (state is null)
        {
            return Unauthorized();
        }

        var payload = JsonSerializer.Deserialize<PasskeyAssertionStatePayload>(state.Payload);
        if (payload is null)
        {
            return Unauthorized();
        }

        var assertionResult = await passkeyHandler.PerformAssertionAsync(new PasskeyAssertionContext
        {
            HttpContext = HttpContext,
            CredentialJson = request.CredentialJson,
            AssertionState = payload.AssertionState
        });

        if (!assertionResult.Succeeded || assertionResult.User is null || assertionResult.Passkey is null)
        {
            return Unauthorized();
        }

        if (assertionResult.User.IsDisabled || (assertionResult.User.LockoutEnd is not null && assertionResult.User.LockoutEnd > DateTimeOffset.UtcNow))
        {
            return Unauthorized();
        }

        var consumed = await flowStateService.TryConsumeAsync(
            request.StateToken,
            AuthFlowPurposes.PasskeyAuthentication,
            cancellationToken);
        if (!consumed)
        {
            return Unauthorized();
        }

        await userManager.AddOrUpdatePasskeyAsync(assertionResult.User, assertionResult.Passkey);
        var tokenPair = await tokenService.IssueAsync(assertionResult.User, cancellationToken);
        return Ok(ToTokenResponse(tokenPair));
    }

    [HttpPost("refresh")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> RefreshAsync(
        [FromBody] RefreshRequest request,
        [FromServices] ITokenService tokenService,
        CancellationToken cancellationToken)
    {
        var rotated = await tokenService.RotateRefreshTokenAsync(request.RefreshToken, cancellationToken);
        return rotated is null
            ? Unauthorized()
            : Ok(ToTokenResponse(rotated));
    }

    [HttpPost("logout")]
    [EnableRateLimiting("auth")]
    public async Task<IActionResult> LogoutAsync(
        [FromBody] LogoutRequest request,
        [FromServices] ITokenService tokenService,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            await tokenService.RevokeRefreshTokenAsync(request.RefreshToken, "logout", cancellationToken);
        }

        return NoContent();
    }

    private static TokenResponse ToTokenResponse(TokenPair pair) =>
        new(pair.AccessToken, pair.AccessTokenExpiresAt, pair.RefreshToken, pair.RefreshTokenExpiresAt);

    private sealed record TwoFactorSetupPayload;
    private sealed record TwoFactorChallengePayload;
    private sealed record PasskeyAssertionStatePayload(string AssertionState);
}

public sealed record PasswordLoginRequest(string UserName, string Password);
public sealed record TwoFactorSetupRequiredResponse(string Status, string SetupToken);
public sealed record TwoFactorChallengeRequiredResponse(string Status, string ChallengeToken);
public sealed record TwoFactorVerifyRequest(string ChallengeToken, string Code);
public sealed record TokenResponse(
    string AccessToken,
    DateTimeOffset AccessTokenExpiresAt,
    string RefreshToken,
    DateTimeOffset RefreshTokenExpiresAt);
public sealed record RefreshRequest(string RefreshToken);
public sealed record LogoutRequest(string? RefreshToken);
public sealed record PasskeyAuthOptionsRequest(string? UserName);
public sealed record PasskeyRequestOptionsResponse(string RequestOptionsJson, string StateToken);
public sealed record PasskeyAuthCompleteRequest(string StateToken, string CredentialJson);
