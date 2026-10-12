using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Takt.Identity.API.IntegrationTests.Infrastructure;
using Takt.Identity.API.Persistence;

namespace Takt.Identity.API.IntegrationTests;

[SetUpFixture]
public sealed class IntegrationTestAssemblySetup
{
    public static IntegrationTestFixture Fixture { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task SetUp()
    {
        Fixture = new IntegrationTestFixture();
        await Fixture.InitializeAsync();
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        await Fixture.DisposeAsync();
    }
}

[NonParallelizable]
public abstract class IntegrationTestBase
{
    protected static IntegrationTestFixture Fixture => IntegrationTestAssemblySetup.Fixture;
    
    protected async Task CreateUser(string username, string password)
    {
        using var scope = Fixture.Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var result = await userManager.CreateAsync(new ApplicationUser()
        {
            UserName = username
        }, password);

        if (!result.Succeeded) throw new InvalidOperationException($"Cannot create user {username}");
    }
}
