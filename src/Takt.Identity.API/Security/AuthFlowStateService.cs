using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Takt.Identity.API.Persistence;

namespace Takt.Identity.API.Security;

public sealed class AuthFlowStateService(
    IdentityAppDbContext dbContext,
    IRandomTokenGenerator tokenGenerator,
    ITokenHasher tokenHasher,
    TimeProvider timeProvider)
    : IAuthFlowStateService
{
    public async Task<string> CreateAsync(
        string purpose,
        long? userId,
        object payload,
        TimeSpan ttl,
        CancellationToken cancellationToken)
    {
        var token = tokenGenerator.Create();
        var state = new AuthFlowState
        {
            Purpose = purpose,
            UserId = userId,
            TokenHash = tokenHasher.HashToken(token),
            Payload = JsonSerializer.Serialize(payload),
            CreatedAt = timeProvider.GetUtcNow(),
            ExpiresAt = timeProvider.GetUtcNow().Add(ttl)
        };

        dbContext.AuthFlowStates.Add(state);
        await dbContext.SaveChangesAsync(cancellationToken);
        return token;
    }

    public Task<AuthFlowState?> GetActiveAsync(string token, string purpose, CancellationToken cancellationToken)
    {
        var tokenHash = tokenHasher.HashToken(token);
        var now = timeProvider.GetUtcNow();
        return dbContext.AuthFlowStates
            .AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.TokenHash == tokenHash &&
                     x.Purpose == purpose &&
                     x.ConsumedAt == null &&
                     x.ExpiresAt > now,
                cancellationToken);
    }

    public async Task<bool> TryConsumeAsync(string token, string purpose, CancellationToken cancellationToken)
    {
        var tokenHash = tokenHasher.HashToken(token);
        var now = timeProvider.GetUtcNow();

        var updated = await dbContext.AuthFlowStates
            .Where(x => x.TokenHash == tokenHash &&
                        x.Purpose == purpose &&
                        x.ConsumedAt == null &&
                        x.ExpiresAt > now)
            .ExecuteUpdateAsync(
                set => set.SetProperty(x => x.ConsumedAt, now),
                cancellationToken);

        return updated == 1;
    }
}
