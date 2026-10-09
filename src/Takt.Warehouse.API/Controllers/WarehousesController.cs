using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Takt.Warehouse.API.Domain;
using Takt.Warehouse.API.Persistence;
using Takt.Warehouse.API.Services;

namespace Takt.Warehouse.API.Controllers;

[Route("warehouses")]
public sealed class WarehousesController(
    IInventoryService inventoryService,
    WarehouseDbContext dbContext) : ApiControllerBase
{
    [HttpPost]
    [Authorize]
    public Task<IActionResult> CreateAsync([FromBody] CreateWarehouseRequest request, CancellationToken cancellationToken)
    {
        return HandleBusinessAsync(async () =>
        {
            var created = await inventoryService.CreateWarehouseAsync(request, cancellationToken);
            return Created($"/warehouses/{created.Id}", created);
        });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAsync([FromQuery] bool? active, [FromQuery] int offset = 0, [FromQuery] int limit = 50, CancellationToken cancellationToken = default)
    {
        if (offset < 0 || limit is < 1 or > 200)
        {
            return BadRequest(new { message = "Invalid pagination parameters." });
        }

        var query = dbContext.Warehouses.AsNoTracking().AsQueryable();
        if (active.HasValue)
        {
            query = query.Where(x => x.IsActive == active.Value);
        }

        var result = await query
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Skip(offset)
            .Take(limit)
            .Select(x => new WarehouseResponse(x.Id, x.BranchId, x.Name, x.Description, x.IsActive))
            .ToListAsync(cancellationToken);

        return Ok(result);
    }

    [HttpGet("{warehouseId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByIdAsync([FromRoute] Guid warehouseId, CancellationToken cancellationToken)
    {
        var warehouse = await dbContext.Warehouses
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == warehouseId, cancellationToken);

        return warehouse is null
            ? NotFound()
            : Ok(new WarehouseResponse(warehouse.Id, warehouse.BranchId, warehouse.Name, warehouse.Description, warehouse.IsActive));
    }

    [HttpPost("{warehouseId:guid}/receipts")]
    [Authorize]
    public Task<IActionResult> ReceiveAsync([FromRoute] Guid warehouseId, [FromBody] ReceiveStockRequest request, CancellationToken cancellationToken)
    {
        return HandleBusinessAsync(async () =>
        {
            var result = await inventoryService.ReceiveAsync(
                warehouseId,
                request,
                RequireIdempotencyKey(),
                ResolveSubject(),
                cancellationToken);
            return StatusCode(result.StatusCode, result.Body);
        });
    }

    [HttpPost("{warehouseId:guid}/issues")]
    [Authorize]
    public Task<IActionResult> IssueAsync([FromRoute] Guid warehouseId, [FromBody] IssueStockRequest request, CancellationToken cancellationToken)
    {
        return HandleBusinessAsync(async () =>
        {
            var result = await inventoryService.IssueAsync(
                warehouseId,
                request,
                RequireIdempotencyKey(),
                ResolveSubject(),
                cancellationToken);
            return StatusCode(result.StatusCode, result.Body);
        });
    }

    [HttpPost("{warehouseId:guid}/returns")]
    [Authorize]
    public Task<IActionResult> ReturnAsync([FromRoute] Guid warehouseId, [FromBody] ReturnStockRequest request, CancellationToken cancellationToken)
    {
        return HandleBusinessAsync(async () =>
        {
            var result = await inventoryService.ReturnAsync(
                warehouseId,
                request,
                RequireIdempotencyKey(),
                ResolveSubject(),
                cancellationToken);
            return StatusCode(result.StatusCode, result.Body);
        });
    }

    [HttpPost("{warehouseId:guid}/transfers")]
    [Authorize]
    public Task<IActionResult> TransferAsync([FromRoute] Guid warehouseId, [FromBody] TransferStockRequest request, CancellationToken cancellationToken)
    {
        return HandleBusinessAsync(async () =>
        {
            var result = await inventoryService.TransferAsync(
                warehouseId,
                request,
                RequireIdempotencyKey(),
                ResolveSubject(),
                cancellationToken);
            return StatusCode(result.StatusCode, result.Body);
        });
    }

    [HttpPost("{warehouseId:guid}/adjustments")]
    [Authorize]
    public Task<IActionResult> AdjustAsync([FromRoute] Guid warehouseId, [FromBody] AdjustStockRequest request, CancellationToken cancellationToken)
    {
        return HandleBusinessAsync(async () =>
        {
            var result = await inventoryService.AdjustAsync(
                warehouseId,
                request,
                RequireIdempotencyKey(),
                ResolveSubject(),
                cancellationToken);
            return StatusCode(result.StatusCode, result.Body);
        });
    }

    [HttpGet("{warehouseId:guid}/stock")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStockAsync([FromRoute] Guid warehouseId, [FromQuery] int offset = 0, [FromQuery] int limit = 100, CancellationToken cancellationToken = default)
    {
        if (offset < 0 || limit is < 1 or > 200)
        {
            return BadRequest(new { message = "Invalid pagination parameters." });
        }

        var stock = await dbContext.StockBalances
            .AsNoTracking()
            .Where(x => x.WarehouseId == warehouseId)
            .OrderBy(x => x.SkuId)
            .Skip(offset)
            .Take(limit)
            .Select(x => new StockBalanceResponse(x.WarehouseId, x.SkuId, x.Quantity))
            .ToListAsync(cancellationToken);

        return Ok(stock);
    }

    [HttpGet("{warehouseId:guid}/stock/{skuId:guid}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetStockBySkuAsync([FromRoute] Guid warehouseId, [FromRoute] Guid skuId, CancellationToken cancellationToken)
    {
        var stock = await dbContext.StockBalances
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.WarehouseId == warehouseId && x.SkuId == skuId, cancellationToken);

        return stock is null
            ? NotFound()
            : Ok(new StockBalanceResponse(stock.WarehouseId, stock.SkuId, stock.Quantity));
    }

    [HttpGet("/stock-movements")]
    [AllowAnonymous]
    public async Task<IActionResult> GetMovementsAsync(
        [FromQuery] Guid? warehouseId,
        [FromQuery] Guid? skuId,
        [FromQuery] string? memberId,
        [FromQuery] MovementType? type,
        [FromQuery] int offset = 0,
        [FromQuery] int limit = 100,
        CancellationToken cancellationToken = default)
    {
        if (offset < 0 || limit is < 1 or > 200)
        {
            return BadRequest(new { message = "Invalid pagination parameters." });
        }

        var query = dbContext.StockMovements.AsNoTracking().AsQueryable();
        if (warehouseId.HasValue)
        {
            query = query.Where(x => x.WarehouseId == warehouseId.Value);
        }

        if (skuId.HasValue)
        {
            query = query.Where(x => x.SkuId == skuId.Value);
        }

        if (!string.IsNullOrWhiteSpace(memberId))
        {
            query = query.Where(x => x.MemberId == memberId);
        }

        if (type.HasValue)
        {
            query = query.Where(x => x.Type == type.Value);
        }

        var movements = await query
            .OrderByDescending(x => x.OccurredAt)
            .ThenBy(x => x.Id)
            .Skip(offset)
            .Take(limit)
            .Select(x => new MovementResponse(
                x.Id,
                x.WarehouseId,
                x.SkuId,
                x.Type,
                x.Quantity,
                x.MemberId,
                x.SourceWarehouseId,
                x.TargetWarehouseId,
                x.Reason,
                x.RelatedMovementId,
                x.OccurredAt))
            .ToListAsync(cancellationToken);

        return Ok(movements);
    }
}
