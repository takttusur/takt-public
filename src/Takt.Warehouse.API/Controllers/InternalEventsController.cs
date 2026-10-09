using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Takt.Warehouse.API.Services;

namespace Takt.Warehouse.API.Controllers;

[Route("internal/events")]
[Authorize]
public sealed class InternalEventsController(IInventoryService inventoryService) : ApiControllerBase
{
    [HttpPost("members")]
    public Task<IActionResult> ProcessMemberEventAsync([FromBody] MemberEventEnvelope request, CancellationToken cancellationToken)
    {
        return HandleBusinessAsync(async () =>
        {
            await inventoryService.ProcessMemberEventAsync(request, cancellationToken);
            return NoContent();
        });
    }
}
