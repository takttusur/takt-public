using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Takt.Identity.API.Bootstrap;
using Takt.Identity.API.Configuration;
using Takt.Identity.API.Constants;
using Takt.Identity.API.IntegrationTests.Infrastructure;
using Takt.Identity.API.Persistence;

namespace Takt.Identity.API.IntegrationTests;

[NonParallelizable]
public sealed class BootstrapTests : IntegrationTestBase
{
    private const string AdminUserName = "admin";
    private const string AdminUserPassword = "Aa123456789!";
    private const string AdminRoleName = "Admin";
    private const string UserRoleName = "User";
    
    [SetUp]
    public async Task SetUp()
    {
        // Db operations should be run one by one, as DbContext doesn't support parallel operations here.
        using var scope = Fixture.Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var existing = await userManager.FindByNameAsync(AdminUserName);
        if (existing is not null)
        {
            await userManager.DeleteAsync(existing);
        }
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<long>>>();
        var adminRoleTask = await roleManager.FindByNameAsync(AdminRoleName);
        var userRoleTask = await roleManager.FindByNameAsync(UserRoleName);
        if (adminRoleTask != null)
        {
            await roleManager.DeleteAsync(adminRoleTask);
        }

        if (userRoleTask != null)
        {
            await roleManager.DeleteAsync(userRoleTask);
        }
    }
    
    [Test]
    public async Task Bootstrap_CreateRolesAndAdminUser()
    {
        using var scope = Fixture.Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<long>>>();
        var bootstrap = scope.ServiceProvider.GetRequiredService<BootstrapService>();
        
        await bootstrap.BootstrapInitialAdminAsync(CancellationToken.None);

        var admin = await userManager.FindByNameAsync(AdminUserName);
        var userRole = await roleManager.FindByNameAsync(UserRoleName);
        var adminRole = await roleManager.FindByNameAsync(AdminRoleName);

        await Assert.MultipleAsync(async () =>
        {
            Assert.That(userRole, Is.Not.Null, "User role should be created");
            Assert.That(adminRole, Is.Not.Null, "Admin role should be created");
            
            Assert.That(admin, Is.Not.Null, "Admin user should be created");
            Assert.That(admin!.TwoFactorEnabled, Is.False, "Admin initially should not have 2FA");

            var roles = await userManager.GetRolesAsync(admin);
            
            Assert.That(roles, Has.Member(AdminRoleName), "Admin user should have Admin role");
            Assert.That(roles, Has.Member(UserRoleName), "Admin user should have User role as well");
            
            var isPasswordValid = await userManager.CheckPasswordAsync(admin, AdminUserPassword);
            Assert.That(isPasswordValid, Is.True, "Admin should have default password initially");
        });
    }

    [Test]
    public async Task Bootstrap_IsIdempotentAndDontChangeData()
    {
        using var scope = Fixture.Factory.Services.CreateScope();
        var bootstrapService = scope.ServiceProvider.GetRequiredService<BootstrapService>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        const string newPassword = "!NewNew12345678";

        await bootstrapService.BootstrapInitialAdminAsync(CancellationToken.None);
        var user = await userManager.FindByNameAsync(AdminUserName);
        var hasDefaultPassword = await userManager.CheckPasswordAsync(user!, AdminUserPassword);
        if (!hasDefaultPassword) throw new InvalidOperationException("User doesn't have default password");

        var result = await userManager.ChangePasswordAsync(user, AdminUserPassword, newPassword);
        if (!result.Succeeded) throw new InvalidOperationException("Cannot change user password");

        await bootstrapService.BootstrapInitialAdminAsync(CancellationToken.None);

        var user1 = await userManager.FindByNameAsync(AdminUserName);
        var hasNewPassword = await userManager.CheckPasswordAsync(user1!, newPassword);
        var roles = await userManager.GetRolesAsync(user1!);
        
        Assert.That(user.Id, Is.EqualTo(user1.Id), "Bootstrap should not create new admins");
        Assert.That(hasNewPassword, Is.True, "Bootstrap should not change password of existing admin");
        Assert.That(roles, Has.Exactly(2).Items, "Admin should have only two roles");
    }
}
