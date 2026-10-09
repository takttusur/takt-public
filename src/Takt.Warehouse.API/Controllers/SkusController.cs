using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Takt.Warehouse.API.Persistence;
using Takt.Warehouse.API.Services;

namespace Takt.Warehouse.API.Controllers;

[Route("skus")]
public sealed class SkusController(IInventoryService inventoryService, WarehouseDbContext dbContext) : ApiControllerBase
{
    [HttpPost]
    [Authorize]
    public Task<IActionResult> CreateAsync([FromBody] CreateSkuRequest request, CancellationToken cancellationToken)
    {
        return HandleBusinessAsync(async () =>
        {
            var created = await inventoryService.CreateSkuAsync(request, cancellationToken);
            return Created($"/skus/{created.Id}", created);
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

        var query = dbContext.Skus.AsNoTracking().AsQueryable();
        if (active.HasValue)
        {
            query = query.Where(x => x.IsActive == active.Value);
        }

        var result = await query
            .OrderBy(x => x.Code)
            .ThenBy(x => x.Id)
            .Skip(offset)
            .Take(limit)
            .Select(x => new SkuResponse(x.Id, x.Code, x.Name, x.Description, x.Category, x.Unit, x.IsActive))
            .ToListAsync(cancellationToken);

        return Ok(result);
    }
}
