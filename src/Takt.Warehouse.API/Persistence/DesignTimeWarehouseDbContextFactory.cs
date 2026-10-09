using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Takt.Warehouse.API.Persistence;

public sealed class DesignTimeWarehouseDbContextFactory : IDesignTimeDbContextFactory<WarehouseDbContext>
{
    public WarehouseDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<WarehouseDbContext>();
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__DefaultConnection")
                               ?? "Host=localhost;Port=5432;Database=takt_warehouse;Username=postgres;Password=postgres";
        optionsBuilder.UseNpgsql(connectionString);
        return new WarehouseDbContext(optionsBuilder.Options);
    }
}
