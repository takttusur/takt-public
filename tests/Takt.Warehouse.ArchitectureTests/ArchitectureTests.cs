using NetArchTest.Rules;

namespace Takt.Warehouse.ArchitectureTests;

public sealed class ArchitectureTests
{
    [Test]
    public void Domain_Should_Not_Depend_On_EntityFramework()
    {
        var result = Types.InAssembly(typeof(Takt.Warehouse.API.Program).Assembly)
            .That()
            .ResideInNamespace("Takt.Warehouse.API.Domain")
            .ShouldNot()
            .HaveDependencyOn("Microsoft.EntityFrameworkCore")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True);
    }

    [Test]
    public void Services_Should_Not_Depend_On_Controllers()
    {
        var result = Types.InAssembly(typeof(Takt.Warehouse.API.Program).Assembly)
            .That()
            .ResideInNamespace("Takt.Warehouse.API.Services")
            .ShouldNot()
            .HaveDependencyOn("Takt.Warehouse.API.Controllers")
            .GetResult();

        Assert.That(result.IsSuccessful, Is.True);
    }
}
