using System.Text.Json;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Takt.Identity.API.Persistence;

namespace Takt.Identity.API.Services;

public sealed class AuthFlowStateService(
    IdentityAppDbContext dbContext,
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
        var token = Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
        var state = new AuthFlowState
        {
            Purpose = purpose,
            UserId = userId,
            TokenHash = HashToken(token),
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
        var tokenHash = HashToken(token);
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
        var tokenHash = HashToken(token);
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

    private static string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
