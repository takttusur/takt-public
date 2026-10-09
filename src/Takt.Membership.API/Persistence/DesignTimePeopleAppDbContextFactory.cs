using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Takt.People.API.Persistence;

public sealed class DesignTimePeopleAppDbContextFactory : IDesignTimeDbContextFactory<PeopleAppDbContext>
{
    public PeopleAppDbContext CreateDbContext(string[] args)
    {
        var builder = new DbContextOptionsBuilder<PeopleAppDbContext>();
        builder.UseNpgsql("Host=localhost;Port=5432;Database=takt_people;Username=postgres;******");
        return new PeopleAppDbContext(builder.Options);
    }
}
