using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using Takt.Identity.API.Configuration;

namespace Takt.Identity.API.Authorization;

public sealed class StrongAuthenticationFreshHandler(
    IOptions<SecurityOptions> securityOptionsAccessor,
    TimeProvider timeProvider)
    : AuthorizationHandler<StrongAuthenticationFreshRequirement>
{
    private readonly SecurityOptions _security = securityOptionsAccessor.Value;

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        StrongAuthenticationFreshRequirement requirement)
    {
        var value = context.User.FindFirstValue("auth_time");
        if (!long.TryParse(value, out var seconds))
        {
            return Task.CompletedTask;
        }

        var authAt = DateTimeOffset.FromUnixTimeSeconds(seconds);
        var age = timeProvider.GetUtcNow() - authAt;
        if (age <= TimeSpan.FromMinutes(_security.StrongAuthenticationFreshnessMinutes))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
