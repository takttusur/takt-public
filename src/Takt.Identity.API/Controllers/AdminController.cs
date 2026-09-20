using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Takt.Identity.API.Constants;
using Takt.Identity.API.Persistence;
using Takt.Identity.API.Security;

namespace Takt.Identity.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/admin")]
[Authorize(Policy = Policies.AdminOnly)]
public sealed class AdminController : ControllerBase
{
    [HttpGet("users")]
    public async Task<IActionResult> ListUsersAsync(
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] IdentityAppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var users = await userManager.Users
            .AsNoTracking()
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(cancellationToken);

        var list = new List<AdminUserResponse>(users.Count);
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            var passkeyCount = await dbContext.Set<IdentityUserPasskey<long>>()
                .CountAsync(x => x.UserId.Equals(user.Id), cancellationToken);
            list.Add(new AdminUserResponse(
                user.Id,
                user.UserName ?? string.Empty,
                user.IsDisabled,
                user.TwoFactorEnabled,
                roles,
                passkeyCount,
                user.CreatedAt));
        }

        return Ok(list);
    }

    [HttpGet("users/{id:long}")]
    public async Task<IActionResult> GetUserAsync(
        [FromRoute] long id,
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] IdentityAppDbContext dbContext,
        CancellationToken cancellationToken)
    {
        var user = await userManager.Users.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        var roles = await userManager.GetRolesAsync(user);
        var passkeyCount = await dbContext.Set<IdentityUserPasskey<long>>()
            .CountAsync(x => x.UserId.Equals(user.Id), cancellationToken);

        return Ok(new AdminUserResponse(
            user.Id,
            user.UserName ?? string.Empty,
            user.IsDisabled,
            user.TwoFactorEnabled,
            roles,
            passkeyCount,
            user.CreatedAt));
    }

    [HttpPost("users/{id:long}/disable")]
    public async Task<IActionResult> DisableUserAsync(
        [FromRoute] long id,
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] IAuditService auditService,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return NotFound();
        }

        var isAdmin = await userManager.IsInRoleAsync(user, SystemRoles.Admin);
        if (isAdmin)
        {
            var activeAdminCount = 0;
            var admins = await userManager.GetUsersInRoleAsync(SystemRoles.Admin);
            foreach (var admin in admins)
            {
                if (!admin.IsDisabled)
                {
                    activeAdminCount++;
                }
            }

            if (activeAdminCount <= 1 && !user.IsDisabled)
            {
                return Problem("Cannot disable the last active administrator.", statusCode: StatusCodes.Status409Conflict);
            }
        }

        user.IsDisabled = true;
        await userManager.UpdateAsync(user);
        await userManager.UpdateSecurityStampAsync(user);

        await auditService.WriteAsync(
            "admin.user.disabled",
            User.GetUserId(),
            user.Id,
            new { user.UserName },
            cancellationToken);

        return NoContent();
    }

    [HttpPost("users/{id:long}/enable")]
    public async Task<IActionResult> EnableUserAsync(
        [FromRoute] long id,
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] IAuditService auditService,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return NotFound();
        }

        user.IsDisabled = false;
        await userManager.UpdateAsync(user);
        await userManager.UpdateSecurityStampAsync(user);

        await auditService.WriteAsync(
            "admin.user.enabled",
            User.GetUserId(),
            user.Id,
            new { user.UserName },
            cancellationToken);

        return NoContent();
    }

    [HttpPost("users/{id:long}/revoke-sessions")]
    public async Task<IActionResult> RevokeSessionsAsync(
        [FromRoute] long id,
        [FromServices] UserManager<ApplicationUser> userManager,
        [FromServices] ITokenService tokenService,
        [FromServices] IAuditService auditService,
        CancellationToken cancellationToken)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user is null)
        {
            return NotFound();
        }

        await tokenService.RevokeAllUserSessionsAsync(user.Id, "admin-revocation", cancellationToken);
        await userManager.UpdateSecurityStampAsync(user);

        await auditService.WriteAsync(
            "admin.user.sessions.revoked",
            User.GetUserId(),
            user.Id,
            new { user.UserName },
            cancellationToken);

        return NoContent();
    }
}

public sealed record AdminUserResponse(
    long Id,
    string UserName,
    bool IsDisabled,
    bool TwoFactorEnabled,
    IList<string> Roles,
    int PasskeyCount,
    DateTimeOffset CreatedAt);
