using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Takt.Identity.API.Security;

namespace Takt.Identity.API.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/.well-known/jwks.json")]
public sealed class JwksController : ControllerBase
{
    [HttpGet]
    public IActionResult Get([FromServices] IJwtSigningKeyProvider keyProvider)
    {
        return Ok(new { keys = new[] { keyProvider.JsonWebKey } });
    }
}
