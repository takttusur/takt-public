using Microsoft.AspNetCore.Mvc;
using Takt.Warehouse.API.Services;

namespace Takt.Warehouse.API.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected async Task<IActionResult> HandleBusinessAsync(Func<Task<IActionResult>> action)
    {
        try
        {
            return await action();
        }
        catch (BusinessException exception)
        {
            var problem = new ProblemDetails
            {
                Title = exception.Title,
                Detail = exception.Message,
                Status = exception.StatusCode,
                Type = $"https://errors.takt.club/{exception.Code}"
            };
            problem.Extensions["code"] = exception.Code;
            return StatusCode(exception.StatusCode, problem);
        }
    }

    protected string RequireIdempotencyKey()
    {
        if (!Request.Headers.TryGetValue("Idempotency-Key", out var idempotencyValues))
        {
            throw new BusinessException(
                "inventory.idempotency_key_required",
                "Idempotency-Key header is required",
                StatusCodes.Status400BadRequest,
                "Mutating operations require Idempotency-Key header.");
        }

        return idempotencyValues.ToString();
    }

    protected string ResolveSubject()
    {
        return User.FindFirst("sub")?.Value
               ?? User.FindFirst("nameid")?.Value
               ?? "system";
    }
}
