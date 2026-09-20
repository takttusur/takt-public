using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Takt.Identity.API.Bootstrap;
using Takt.Identity.API.Constants;
using Takt.Identity.API.Persistence;
using Takt.Identity.API.Tests.Infrastructure;

namespace Takt.Identity.API.Tests;

[Collection(IntegrationCollection.Name)]
public sealed class BootstrapTests(IntegrationTestFixture fixture)
{
    [Fact]
    public async Task Bootstrap_CreatesAdmin_AndIsIdempotent()
    {
        using var scope = fixture.Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var bootstrap = scope.ServiceProvider.GetRequiredService<BootstrapService>();

        var existing = await userManager.FindByNameAsync("admin");
        if (existing is not null)
        {
            await userManager.DeleteAsync(existing);
        }

        var writer = new StringWriter();
        var originalOut = Console.Out;
        Console.SetOut(writer);
        try
        {
            await bootstrap.BootstrapInitialAdminAsync(CancellationToken.None);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        var output = writer.ToString();
        Assert.Contains("INITIAL ADMINISTRATOR CREATED", output);
        Assert.Contains("Username: admin", output);

        var passwordLine = output.Split(Environment.NewLine)
            .Single(line => line.StartsWith("Password:", StringComparison.Ordinal));
        var password = passwordLine.Split(':', 2)[1].Trim();
        Assert.NotEmpty(password);

        var admin = await userManager.FindByNameAsync("admin");
        Assert.NotNull(admin);
        Assert.False(admin!.TwoFactorEnabled);
        Assert.True(await userManager.IsInRoleAsync(admin, SystemRoles.Admin));
        Assert.True(await userManager.CheckPasswordAsync(admin, password));

        var initialHash = admin.PasswordHash;

        writer.GetStringBuilder().Clear();
        Console.SetOut(writer);
        try
        {
            await bootstrap.BootstrapInitialAdminAsync(CancellationToken.None);
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        var secondOutput = writer.ToString();
        Assert.DoesNotContain("Password:", secondOutput);

        var adminAfterSecondRun = await userManager.FindByNameAsync("admin");
        Assert.Equal(initialHash, adminAfterSecondRun?.PasswordHash);
    }
}
