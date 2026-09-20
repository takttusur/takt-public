using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Takt.Identity.API.Bootstrap;
using Takt.Identity.API.Configuration;
using Takt.Identity.API.Constants;
using Takt.Identity.API.IntegrationTests.Infrastructure;
using Takt.Identity.API.Persistence;

namespace Takt.Identity.API.IntegrationTests;

public sealed class BootstrapTests : IntegrationTestBase
{
    [Test]
    public async Task Bootstrap_CreatesAdmin_AndIsIdempotent()
    {
        using var scope = Fixture.Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var bootstrap = scope.ServiceProvider.GetRequiredService<BootstrapService>();
        var options = scope.ServiceProvider.GetRequiredService<IOptions<BootstrapOptions>>().Value;

        var existing = await userManager.FindByNameAsync("admin");
        if (existing is not null)
        {
            await userManager.DeleteAsync(existing);
        }

        await bootstrap.BootstrapInitialAdminAsync(CancellationToken.None);

        var admin = await userManager.FindByNameAsync("admin");
        Assert.That(admin, Is.Not.Null);
        Assert.That(admin!.TwoFactorEnabled, Is.False);
        Assert.That(await userManager.IsInRoleAsync(admin, SystemRoles.Admin), Is.True);
        Assert.That(await userManager.CheckPasswordAsync(admin, options.AdminInitialPassword), Is.True);

        var initialHash = admin.PasswordHash;

        await bootstrap.BootstrapInitialAdminAsync(CancellationToken.None);

        var adminAfterSecondRun = await userManager.FindByNameAsync("admin");
        Assert.That(adminAfterSecondRun?.PasswordHash, Is.EqualTo(initialHash));
    }
}
