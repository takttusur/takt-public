using Microsoft.IdentityModel.Tokens;

namespace Takt.Identity.API.Security;

public interface IJwtSigningKeyProvider
{
    SigningCredentials SigningCredentials { get; }
    SecurityKey SigningKey { get; }
    JsonWebKey JsonWebKey { get; }
}
