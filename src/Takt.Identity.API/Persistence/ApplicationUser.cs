using Microsoft.AspNetCore.Identity;

namespace Takt.Identity.API.Persistence;

public sealed class ApplicationUser : IdentityUser<long>
{
    public bool IsDisabled { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
