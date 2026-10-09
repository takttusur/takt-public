using System.ComponentModel.DataAnnotations;
using Takt.Warehouse.API.Domain;

namespace Takt.Warehouse.API.Persistence;

public sealed class Sku
{
    public Guid Id { get; set; }
    [MaxLength(64)] public string Code { get; set; } = string.Empty;
    [MaxLength(256)] public string Name { get; set; } = string.Empty;
    [MaxLength(2000)] public string? Description { get; set; }
    [MaxLength(128)] public string? Category { get; set; }
    [MaxLength(32)] public string Unit { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class WarehouseEntity
{
    public Guid Id { get; set; }
    [MaxLength(64)] public string BranchId { get; set; } = string.Empty;
    [MaxLength(256)] public string Name { get; set; } = string.Empty;
    [MaxLength(2000)] public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class StockBalance
{
    public Guid WarehouseId { get; set; }
    public Guid SkuId { get; set; }
    public int Quantity { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class StockMovement
{
    public Guid Id { get; set; }
    public Guid WarehouseId { get; set; }
    public Guid SkuId { get; set; }
    public MovementType Type { get; set; }
    public int Quantity { get; set; }
    [MaxLength(128)] public string? MemberId { get; set; }
    public Guid? SourceWarehouseId { get; set; }
    public Guid? TargetWarehouseId { get; set; }
    [MaxLength(1024)] public string? Reason { get; set; }
    public Guid? RelatedMovementId { get; set; }
    [MaxLength(128)] public string CreatedBy { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
}

public sealed class InventoryMember
{
    [MaxLength(128)] public string MemberId { get; set; } = string.Empty;
    [MaxLength(64)] public string BranchId { get; set; } = string.Empty;
    public InventoryMemberStatus Status { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}

public sealed class OutboxMessage
{
    public Guid Id { get; set; }
    [MaxLength(256)] public string EventType { get; set; } = string.Empty;
    public string Payload { get; set; } = string.Empty;
    public DateTimeOffset OccurredAt { get; set; }
    public DateTimeOffset? PublishedAt { get; set; }
}

public sealed class InboxMessage
{
    [MaxLength(128)] public string MessageId { get; set; } = string.Empty;
    [MaxLength(256)] public string MessageType { get; set; } = string.Empty;
    public DateTimeOffset ProcessedAt { get; set; }
}

public sealed class IdempotencyRecord
{
    public Guid Id { get; set; }
    [MaxLength(128)] public string Key { get; set; } = string.Empty;
    [MaxLength(128)] public string Operation { get; set; } = string.Empty;
    [MaxLength(128)] public string RequestHash { get; set; } = string.Empty;
    public int StatusCode { get; set; }
    public string ResponseJson { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }
}
