using System.Security.Cryptography;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Takt.Identity.API.Configuration;
using Takt.Identity.API.Constants;
using Takt.Identity.API.Persistence;

namespace Takt.Identity.API.Bootstrap;

public sealed class BootstrapService(
    UserManager<ApplicationUser> userManager,
    RoleManager<IdentityRole<long>> roleManager,
    IOptions<BootstrapOptions> optionsAccessor)
{
    public async Task BootstrapInitialAdminAsync(CancellationToken cancellationToken)
    {
        var options = optionsAccessor.Value;
        foreach (var role in SystemRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var roleResult = await roleManager.CreateAsync(new IdentityRole<long>(role));
                if (!roleResult.Succeeded)
                {
                    throw new InvalidOperationException($"Role creation failed: {string.Join(", ", roleResult.Errors.Select(x => x.Description))}");
                }
            }
        }

        var existing = await userManager.FindByNameAsync(options.AdminUserName);
        if (existing is not null)
        {
            return;
        }

        var password = CreatePassword();
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

        Console.WriteLine("========================================");
        Console.WriteLine("INITIAL ADMINISTRATOR CREATED");
        Console.WriteLine();
        Console.WriteLine($"Username: {user.UserName}");
        Console.WriteLine($"Password: {password}");
        Console.WriteLine();
        Console.WriteLine("IMPORTANT:");
        Console.WriteLine("Save this password securely.");
        Console.WriteLine("It will not be displayed again.");
        Console.WriteLine("TOTP setup is required on first login.");
        Console.WriteLine("========================================");
    }

    private static string CreatePassword()
    {
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string special = "!@$%^&*()-_=+";
        const string all = upper + lower + digits + special;

        var chars = new List<char>(24)
        {
            Pick(upper),
            Pick(lower),
            Pick(digits),
            Pick(special)
        };

        for (var i = chars.Count; i < 24; i++)
        {
            chars.Add(Pick(all));
        }

        Shuffle(chars);
        return new string(chars.ToArray());
    }

    private static char Pick(string chars)
    {
        var index = RandomNumberGenerator.GetInt32(chars.Length);
        return chars[index];
    }

    private static void Shuffle(IList<char> chars)
    {
        for (var i = chars.Count - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }
    }
}
