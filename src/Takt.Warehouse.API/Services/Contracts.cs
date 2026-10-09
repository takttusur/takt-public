using Takt.Warehouse.API.Domain;

namespace Takt.Warehouse.API.Services;

public sealed record CreateSkuRequest(string Code, string Name, string? Description, string? Category, string Unit);
public sealed record CreateWarehouseRequest(string BranchId, string Name, string? Description);
public sealed record ReceiveStockRequest(Guid SkuId, int Quantity, string? Reason);
public sealed record IssueStockRequest(string MemberId, Guid SkuId, int Quantity);
public sealed record ReturnStockRequest(string MemberId, Guid SkuId, int Quantity, Guid? IssueMovementId);
public sealed record TransferStockRequest(Guid TargetWarehouseId, Guid SkuId, int Quantity, string? Reason);
public sealed record AdjustStockRequest(Guid SkuId, int Quantity, string Reason);

public sealed record MemberEventEnvelope(string MessageId, string EventType, DateTimeOffset OccurredAt, string MemberId, string BranchId);
public sealed record UpsertMemberRequest(string MemberId, string BranchId, InventoryMemberStatus Status);

public sealed record ApiResult(int StatusCode, object Body);

public sealed record SkuResponse(Guid Id, string Code, string Name, string? Description, string? Category, string Unit, bool IsActive);
public sealed record WarehouseResponse(Guid Id, string BranchId, string Name, string? Description, bool IsActive);
public sealed record StockBalanceResponse(Guid WarehouseId, Guid SkuId, int Quantity);
public sealed record MovementResponse(
    Guid Id,
    Guid WarehouseId,
    Guid SkuId,
    MovementType Type,
    int Quantity,
    string? MemberId,
    Guid? SourceWarehouseId,
    Guid? TargetWarehouseId,
    string? Reason,
    Guid? RelatedMovementId,
    DateTimeOffset OccurredAt);

public sealed record MemberInventoryItemResponse(string MemberId, Guid SkuId, int Quantity);
