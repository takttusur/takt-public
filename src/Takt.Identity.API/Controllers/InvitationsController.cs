using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Takt.Identity.API.Configuration;
using Takt.Identity.API.Constants;
using Takt.Identity.API.Persistence;
using Takt.Identity.API.Security;

namespace Takt.Identity.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/invitations")]
public sealed class InvitationsController : ControllerBase
{
    [HttpPost]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> CreateInvitationAsync(
        [FromBody] CreateInvitationRequest request,
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] IdentityAppDbContext dbContext,
        [FromServices] IRandomTokenGenerator tokenGenerator,
        [FromServices] ITokenHasher tokenHasher,
        [FromServices] IOptions<InvitationOptions> optionsAccessor,
        CancellationToken cancellationToken)
    {
        var normalizedUserName = userManager.NormalizeName(request.UserName);
        if (string.IsNullOrWhiteSpace(normalizedUserName))
        {
            return BadRequest(new ValidationProblemDetails(new Dictionary<string, string[]>
            {
                ["userName"] = ["Username is required."]
            }));
        }

        var existingUser = await userManager.FindByNameAsync(request.UserName);
        if (existingUser is not null)
        {
            return Conflict(new { message = "Username already exists." });
        }

        var role = request.Role == SystemRoles.Admin ? SystemRoles.Admin : SystemRoles.User;
        var token = tokenGenerator.Create(48);
        var now = DateTimeOffset.UtcNow;
        var invitation = new Invitation
        {
            UserName = request.UserName,
            NormalizedUserName = normalizedUserName,
            TokenHash = tokenHasher.HashToken(token),
            Role = role,
            CreatedAt = now,
            ExpiresAt = now.AddHours(optionsAccessor.Value.LifetimeHours),
            CreatedBy = User.GetUserId()
        };

        dbContext.Invitations.Add(invitation);
        await dbContext.SaveChangesAsync(cancellationToken);

        var activationUrl = string.IsNullOrWhiteSpace(optionsAccessor.Value.ActivationBaseUrl)
            ? null
            : $"{optionsAccessor.Value.ActivationBaseUrl.TrimEnd('/')}/{token}";

        return Ok(new CreateInvitationResponse(
            invitation.Id,
            invitation.UserName,
            invitation.Role,
            invitation.ExpiresAt,
            token,
            activationUrl));
    }

    [HttpGet("{token}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetInvitationAsync(
        [FromRoute] string token,
        [FromServices] IdentityAppDbContext dbContext,
        [FromServices] ITokenHasher tokenHasher,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var hash = tokenHasher.HashToken(token);
        var invitation = await dbContext.Invitations
            .AsNoTracking()
            .SingleOrDefaultAsync(x =>
                    x.TokenHash == hash &&
                    x.ConsumedAt == null &&
                    x.RevokedAt == null &&
                    x.ExpiresAt > now,
                cancellationToken);

        return invitation is null
            ? NotFound()
            : Ok(new InvitationStatusResponse(invitation.UserName, invitation.ExpiresAt));
    }

    [HttpPost("{token}/accept")]
    [AllowAnonymous]
    public async Task<IActionResult> AcceptInvitationAsync(
        [FromRoute] string token,
        [FromBody] AcceptInvitationRequest request,
        [FromServices] IdentityAppDbContext dbContext,
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] ITokenHasher tokenHasher,
        [FromServices] IAuthFlowStateService authFlowStateService,
        [FromServices] IOptions<SecurityOptions> securityOptionsAccessor,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var tokenHash = tokenHasher.HashToken(token);

        await using var tx = await dbContext.Database.BeginTransactionAsync(cancellationToken);
        var invitation = await dbContext.Invitations
            .SingleOrDefaultAsync(x => x.TokenHash == tokenHash, cancellationToken);

        if (invitation is null || invitation.ConsumedAt is not null || invitation.RevokedAt is not null || invitation.ExpiresAt <= now)
        {
            return NotFound();
        }

        var consumed = await dbContext.Invitations
            .Where(x => x.Id == invitation.Id && x.ConsumedAt == null && x.RevokedAt == null && x.ExpiresAt > now)
            .ExecuteUpdateAsync(set => set.SetProperty(x => x.ConsumedAt, now), cancellationToken);

        if (consumed != 1)
        {
            return Conflict(new { message = "Invitation is no longer valid." });
        }

        var user = new ApplicationUser
        {
            UserName = invitation.UserName,
            LockoutEnabled = true,
            CreatedAt = now
        };
        var userResult = await userManager.CreateAsync(user, request.Password);
        if (!userResult.Succeeded)
        {
            await tx.RollbackAsync(cancellationToken);
            return BadRequest(new ValidationProblemDetails(ToProblemDictionary(userResult.Errors)));
        }

        var userRole = invitation.Role == SystemRoles.Admin ? SystemRoles.Admin : SystemRoles.User;
        var roleResult = await userManager.AddToRoleAsync(user, userRole);
        if (!roleResult.Succeeded)
        {
            await tx.RollbackAsync(cancellationToken);
            return BadRequest(new ValidationProblemDetails(ToProblemDictionary(roleResult.Errors)));
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        await tx.CommitAsync(cancellationToken);

        var setupToken = await authFlowStateService.CreateAsync(
            AuthFlowPurposes.TwoFactorSetup,
            user.Id,
            new { invitation = invitation.Id },
            TimeSpan.FromMinutes(securityOptionsAccessor.Value.AuthenticationChallengeTtlMinutes),
            cancellationToken);

        return Ok(new AcceptInvitationResponse("TwoFactorSetupRequired", setupToken));
    }

    [HttpDelete("{id:long}")]
    [Authorize(Policy = Policies.AdminOnly)]
    public async Task<IActionResult> RevokeInvitationAsync(
        [FromRoute] long id,
        [FromServices] IdentityAppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
        var updated = await dbContext.Invitations
            .Where(x => x.Id == id && x.ConsumedAt == null && x.RevokedAt == null)
            .ExecuteUpdateAsync(set => set.SetProperty(x => x.RevokedAt, now), cancellationToken);

        return updated == 0 ? NotFound() : NoContent();
    }

    private static Dictionary<string, string[]> ToProblemDictionary(IEnumerable<IdentityError> errors)
    {
        return errors
            .GroupBy(e => string.IsNullOrWhiteSpace(e.Code) ? "Identity" : e.Code)
            .ToDictionary(group => group.Key, group => group.Select(x => x.Description).ToArray());
    }
}

public sealed record CreateInvitationRequest(string UserName, string Role);
public sealed record CreateInvitationResponse(
    long Id,
    string UserName,
    string Role,
    DateTimeOffset ExpiresAt,
    string Token,
    string? ActivationUrl);
public sealed record InvitationStatusResponse(string UserName, DateTimeOffset ExpiresAt);
public sealed record AcceptInvitationRequest(string Password);
public sealed record AcceptInvitationResponse(string Status, string SetupToken);
