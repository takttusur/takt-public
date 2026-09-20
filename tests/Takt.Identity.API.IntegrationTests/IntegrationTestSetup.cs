using Takt.Identity.API.IntegrationTests.Infrastructure;

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
}
