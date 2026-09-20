using Microsoft.IdentityModel.Tokens;

namespace Takt.Identity.API.Services;

public interface IJwtSigningKeyProvider
{
    SigningCredentials SigningCredentials { get; }
    SecurityKey SigningKey { get; }
    JsonWebKey JsonWebKey { get; }
}
