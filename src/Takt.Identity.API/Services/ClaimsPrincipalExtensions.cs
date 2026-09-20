using System.Security.Claims;

namespace Takt.Identity.API.Services;

public static class ClaimsPrincipalExtensions
{
    public static long? GetUserId(this ClaimsPrincipal principal)
    {
        var sub = principal.FindFirstValue("sub");
        return long.TryParse(sub, out var id) ? id : null;
    }
}
