namespace Takt.Identity.API.Persistence;

public sealed class AuthFlowState
{
    public long Id { get; set; }
    public string TokenHash { get; set; } = string.Empty;
    public long? UserId { get; set; }
    public string Purpose { get; set; } = string.Empty;
    public string Payload { get; set; } = "{}";
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset? ConsumedAt { get; set; }
}
