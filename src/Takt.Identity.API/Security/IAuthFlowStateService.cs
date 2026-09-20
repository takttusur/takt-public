using Takt.Identity.API.Persistence;

namespace Takt.Identity.API.Security;

public interface IAuthFlowStateService
{
    Task<string> CreateAsync(string purpose, long? userId, object payload, TimeSpan ttl, CancellationToken cancellationToken);
    Task<AuthFlowState?> GetActiveAsync(string token, string purpose, CancellationToken cancellationToken);
    Task<bool> TryConsumeAsync(string token, string purpose, CancellationToken cancellationToken);
}
