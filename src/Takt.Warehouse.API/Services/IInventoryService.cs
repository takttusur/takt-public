namespace Takt.Warehouse.API.Services;

public interface IInventoryService
{
    Task<SkuResponse> CreateSkuAsync(CreateSkuRequest request, CancellationToken cancellationToken);
    Task<WarehouseResponse> CreateWarehouseAsync(CreateWarehouseRequest request, CancellationToken cancellationToken);
    Task<ApiResult> ReceiveAsync(Guid warehouseId, ReceiveStockRequest request, string idempotencyKey, string createdBy, CancellationToken cancellationToken);
    Task<ApiResult> IssueAsync(Guid warehouseId, IssueStockRequest request, string idempotencyKey, string createdBy, CancellationToken cancellationToken);
    Task<ApiResult> ReturnAsync(Guid warehouseId, ReturnStockRequest request, string idempotencyKey, string createdBy, CancellationToken cancellationToken);
    Task<ApiResult> TransferAsync(Guid warehouseId, TransferStockRequest request, string idempotencyKey, string createdBy, CancellationToken cancellationToken);
    Task<ApiResult> AdjustAsync(Guid warehouseId, AdjustStockRequest request, string idempotencyKey, string createdBy, CancellationToken cancellationToken);
    Task UpsertMemberProjectionAsync(UpsertMemberRequest request, CancellationToken cancellationToken);
    Task ProcessMemberEventAsync(MemberEventEnvelope request, CancellationToken cancellationToken);
}
