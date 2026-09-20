using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Takt.Identity.API.Persistence;

public sealed class DesignTimeIdentityAppDbContextFactory : IDesignTimeDbContextFactory<IdentityAppDbContext>
{
    public IdentityAppDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<IdentityAppDbContext>();
        builder.UseNpgsql("Host=localhost;Port=5432;Database=takt_identity;Username=postgres;Password=postgres");
        return new IdentityAppDbContext(builder.Options);
    }
}
