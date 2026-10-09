using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Takt.Warehouse.API.Domain;
using Takt.Warehouse.API.Persistence;
using Takt.Warehouse.API.Services;

namespace Takt.Warehouse.API.Controllers;

[Route("members")]
public sealed class MembersController(
    IInventoryService inventoryService,
    WarehouseDbContext dbContext) : ApiControllerBase
{
    [HttpPut("{memberId}")]
    [Authorize]
    public Task<IActionResult> UpsertAsync([FromRoute] string memberId, [FromBody] UpsertMemberProjectionBody body, CancellationToken cancellationToken)
    {
        return HandleBusinessAsync(async () =>
        {
            await inventoryService.UpsertMemberProjectionAsync(
                new UpsertMemberRequest(memberId, body.BranchId, body.Status),
                cancellationToken);
            return NoContent();
        });
    }

    [HttpGet("{memberId}/issues")]
    [AllowAnonymous]
    public async Task<IActionResult> GetIssuesAsync([FromRoute] string memberId, [FromQuery] int offset = 0, [FromQuery] int limit = 100, CancellationToken cancellationToken = default)
    {
        if (offset < 0 || limit is < 1 or > 200)
        {
            return BadRequest(new { message = "Invalid pagination parameters." });
        }

        var issues = await dbContext.StockMovements
            .AsNoTracking()
            .Where(x => x.MemberId == memberId && x.Type == MovementType.Issue)
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

        return Ok(issues);
    }

    [HttpGet("{memberId}/inventory")]
    [AllowAnonymous]
    public async Task<IActionResult> GetInventoryAsync([FromRoute] string memberId, CancellationToken cancellationToken)
    {
        var issued = dbContext.StockMovements
            .AsNoTracking()
            .Where(x => x.MemberId == memberId && x.Type == MovementType.Issue)
            .GroupBy(x => x.SkuId)
            .Select(x => new { x.Key, Quantity = x.Sum(y => y.Quantity) });

        var returned = dbContext.StockMovements
            .AsNoTracking()
            .Where(x => x.MemberId == memberId && x.Type == MovementType.Return)
            .GroupBy(x => x.SkuId)
            .Select(x => new { x.Key, Quantity = x.Sum(y => y.Quantity) });

        var issuedList = await issued.ToListAsync(cancellationToken);
        var returnedMap = await returned.ToDictionaryAsync(x => x.Key, x => x.Quantity, cancellationToken);

        var inventory = issuedList
            .Select(x => new MemberInventoryItemResponse(memberId, x.Key, x.Quantity - (returnedMap.TryGetValue(x.Key, out var qty) ? qty : 0)))
            .Where(x => x.Quantity > 0)
            .OrderBy(x => x.SkuId)
            .ToArray();

        return Ok(inventory);
    }
}

public sealed record UpsertMemberProjectionBody(string BranchId, InventoryMemberStatus Status);
