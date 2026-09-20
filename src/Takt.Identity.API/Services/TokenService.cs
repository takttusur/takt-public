using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Takt.Identity.API.Persistence;
using AppTokenOptions = Takt.Identity.API.Configuration.TokenOptions;

namespace Takt.Identity.API.Services;

public sealed class TokenService(
    UserManager<ApplicationUser> userManager,
    IdentityAppDbContext dbContext,
    IJwtSigningKeyProvider signingKeyProvider,
    IRandomTokenGenerator tokenGenerator,
    ITokenHasher tokenHasher,
    IOptions<AppTokenOptions> tokenOptionsAccessor,
    TimeProvider timeProvider) : ITokenService
{
    private readonly AppTokenOptions _options = tokenOptionsAccessor.Value;

    public async Task<TokenPair> IssueAsync(ApplicationUser user, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var accessExpiresAt = now.AddMinutes(_options.AccessTokenLifetimeMinutes);
        var refreshExpiresAt = now.AddDays(_options.RefreshTokenLifetimeDays);
        var roles = await userManager.GetRolesAsync(user);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new("preferred_username", user.UserName ?? string.Empty),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")),
            new("auth_time", now.ToUnixTimeSeconds().ToString())
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var jwt = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: now.UtcDateTime,
            expires: accessExpiresAt.UtcDateTime,
            signingCredentials: signingKeyProvider.SigningCredentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);
        var refreshToken = tokenGenerator.Create(48);
        var refreshHash = tokenHasher.HashToken(refreshToken);

        var refreshSession = new RefreshSession
        {
            UserId = user.Id,
            TokenHash = refreshHash,
            CreatedAt = now,
            ExpiresAt = refreshExpiresAt
        };

        dbContext.RefreshSessions.Add(refreshSession);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new TokenPair(accessToken, accessExpiresAt, refreshToken, refreshExpiresAt);
    }

    public async Task<TokenPair?> RotateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var refreshHash = tokenHasher.HashToken(refreshToken);

        var session = await dbContext.RefreshSessions
            .SingleOrDefaultAsync(x => x.TokenHash == refreshHash, cancellationToken);

        if (session is null || session.RevokedAt is not null || session.ExpiresAt <= now)
        {
            return null;
        }

        var user = await userManager.FindByIdAsync(session.UserId.ToString());
        if (user is null || user.IsDisabled)
        {
            return null;
        }

        if (user.LockoutEnd is not null && user.LockoutEnd > now)
        {
            return null;
        }

        session.RevokedAt = now;
        session.RevokeReason = "rotated";

        var newPair = await IssueAsync(user, cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
        return newPair;
    }

    public async Task RevokeRefreshTokenAsync(string refreshToken, string reason, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var refreshHash = tokenHasher.HashToken(refreshToken);

        var updated = await dbContext.RefreshSessions
            .Where(x => x.TokenHash == refreshHash && x.RevokedAt == null)
            .ExecuteUpdateAsync(
                set => set
                    .SetProperty(x => x.RevokedAt, now)
                    .SetProperty(x => x.RevokeReason, reason),
                cancellationToken);

        if (updated > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
    }

    public async Task RevokeAllUserSessionsAsync(long userId, string reason, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        await dbContext.RefreshSessions
            .Where(x => x.UserId == userId && x.RevokedAt == null)
            .ExecuteUpdateAsync(
                set => set
                    .SetProperty(x => x.RevokedAt, now)
                    .SetProperty(x => x.RevokeReason, reason),
                cancellationToken);
    }
}
