using Takt.Identity.API.Persistence;

namespace Takt.Identity.API.Security;

public interface ITokenService
{
    Task<TokenPair> IssueAsync(ApplicationUser user, CancellationToken cancellationToken);
    Task<TokenPair?> RotateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
    Task RevokeRefreshTokenAsync(string refreshToken, string reason, CancellationToken cancellationToken);
    Task RevokeAllUserSessionsAsync(long userId, string reason, CancellationToken cancellationToken);
}
