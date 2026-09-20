using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Takt.Identity.API.Configuration;
using Takt.Identity.API.Constants;
using Takt.Identity.API.Persistence;

namespace Takt.Identity.API.Bootstrap;

public sealed class BootstrapService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<long>> roleManager,
    IOptions<BootstrapOptions> optionsAccessor,
    ILogger<BootstrapService> logger)
{
    public async Task BootstrapInitialAdminAsync(CancellationToken cancellationToken)
    {
        var options = optionsAccessor.Value;
        foreach (var role in SystemRoles.All)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (await roleManager.RoleExistsAsync(role)) continue;
            
            var roleResult = await roleManager.CreateAsync(new IdentityRole<long>(role));
            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException($"Role creation failed: {string.Join(", ", roleResult.Errors.Select(x => x.Description))}");
            }
        }

        var existing = await userManager.FindByNameAsync(options.AdminUserName);
        if (existing is not null)
        {
            return;
        }

        var password = options.AdminInitialPassword;
        var user = new ApplicationUser
        {
            UserName = options.AdminUserName,
            IsDisabled = false,
            CreatedAt = DateTimeOffset.UtcNow,
            LockoutEnabled = true,
            TwoFactorEnabled = false
        };

        var createResult = await userManager.CreateAsync(user, password);
        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException($"Admin creation failed: {string.Join(", ", createResult.Errors.Select(x => x.Description))}");
        }

        var roleAssignResult = await userManager.AddToRoleAsync(user, SystemRoles.Admin);
        if (!roleAssignResult.Succeeded)
        {
            throw new InvalidOperationException($"Admin role assignment failed: {string.Join(", ", roleAssignResult.Errors.Select(x => x.Description))}");
        }

        logger.LogInformation("User created: {user}", user.UserName);
    }
}
