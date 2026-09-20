using System.Security.Claims;
using Microsoft.Extensions.Options;
using Takt.Identity.API.Configuration;

namespace Takt.Identity.API.Services;

public sealed class StrongAuthenticationPolicy(IOptions<SecurityOptions> securityOptions, TimeProvider timeProvider)
{
    private readonly SecurityOptions _security = securityOptions.Value;

    public bool IsFresh(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue("auth_time");
        if (!long.TryParse(value, out var seconds))
        {
            return false;
        }

        var authAt = DateTimeOffset.FromUnixTimeSeconds(seconds);
        var age = timeProvider.GetUtcNow() - authAt;
        return age <= TimeSpan.FromMinutes(_security.StrongAuthenticationFreshnessMinutes);
    }
}
