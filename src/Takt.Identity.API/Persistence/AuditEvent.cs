namespace Takt.Identity.API.Persistence;

public sealed class AuditEvent
{
    public long Id { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public long? ActorUserId { get; set; }
    public long? SubjectUserId { get; set; }
    public string EventType { get; set; } = string.Empty;
    public string Metadata { get; set; } = "{}";
}
