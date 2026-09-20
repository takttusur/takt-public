using System.Text.Json;
using Takt.Identity.API.Persistence;

namespace Takt.Identity.API.Security;

public sealed class AuditService(IdentityAppDbContext dbContext) : IAuditService
{
    public async Task WriteAsync(
        string eventType,
        long? actorUserId,
        long? subjectUserId,
        object metadata,
        CancellationToken cancellationToken)
    {
        var entry = new AuditEvent
        {
            EventType = eventType,
            ActorUserId = actorUserId,
            SubjectUserId = subjectUserId,
            Metadata = JsonSerializer.Serialize(metadata),
            CreatedAt = DateTimeOffset.UtcNow
        };

        dbContext.AuditEvents.Add(entry);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
