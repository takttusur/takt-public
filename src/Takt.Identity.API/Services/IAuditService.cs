namespace Takt.Identity.API.Services;

public interface IAuditService
{
    Task WriteAsync(
        string eventType,
        long? actorUserId,
        long? subjectUserId,
        object metadata,
        CancellationToken cancellationToken);
}
