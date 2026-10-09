using Microsoft.EntityFrameworkCore;
using Takt.Warehouse.API.Domain;

namespace Takt.Warehouse.API.Persistence;

public sealed class WarehouseDbContext(DbContextOptions<WarehouseDbContext> options) : DbContext(options)
{
    public DbSet<Sku> Skus => Set<Sku>();
    public DbSet<WarehouseEntity> Warehouses => Set<WarehouseEntity>();
    public DbSet<StockBalance> StockBalances => Set<StockBalance>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<InventoryMember> InventoryMembers => Set<InventoryMember>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();
    public DbSet<InboxMessage> InboxMessages => Set<InboxMessage>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Sku>(entity =>
        {
            entity.ToTable("skus");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<WarehouseEntity>(entity =>
        {
            entity.ToTable("warehouses");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Name);
            entity.HasIndex(x => x.BranchId);
            entity.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<StockBalance>(entity =>
        {
            entity.ToTable("stock_balances");
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_stock_balances_quantity_non_negative", "\"Quantity\" >= 0");
            });
            entity.HasKey(x => new { x.WarehouseId, x.SkuId });
            entity.HasIndex(x => x.WarehouseId);
            entity.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<StockMovement>(entity =>
        {
            entity.ToTable("stock_movements");
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_stock_movements_quantity_positive", "\"Quantity\" > 0");
            });
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Type).HasConversion<int>();
            entity.Property(x => x.OccurredAt).HasColumnType("timestamp with time zone");
            entity.HasIndex(x => new { x.WarehouseId, x.SkuId, x.OccurredAt });
            entity.HasIndex(x => x.MemberId);
            entity.HasIndex(x => x.RelatedMovementId);
        });

        modelBuilder.Entity<InventoryMember>(entity =>
        {
            entity.ToTable("inventory_members");
            entity.HasKey(x => x.MemberId);
            entity.Property(x => x.Status).HasConversion<int>();
            entity.HasIndex(x => x.BranchId);
            entity.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<OutboxMessage>(entity =>
        {
            entity.ToTable("outbox_messages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.OccurredAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.PublishedAt).HasColumnType("timestamp with time zone");
            entity.HasIndex(x => new { x.PublishedAt, x.OccurredAt });
        });

        modelBuilder.Entity<InboxMessage>(entity =>
        {
            entity.ToTable("inbox_messages");
            entity.HasKey(x => x.MessageId);
            entity.Property(x => x.ProcessedAt).HasColumnType("timestamp with time zone");
        });

        modelBuilder.Entity<IdempotencyRecord>(entity =>
        {
            entity.ToTable("idempotency_records");
            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.Key, x.Operation }).IsUnique();
            entity.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
        });
    }
}
