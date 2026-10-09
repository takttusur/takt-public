using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Takt.Warehouse.API.Configuration;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string SigningKey { get; init; } = string.Empty;

    public SymmetricSecurityKey CreateSigningKey()
    {
        if (string.IsNullOrWhiteSpace(SigningKey))
        {
            throw new InvalidOperationException("Jwt:SigningKey must be configured.");
        }

        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey));
    }
}
