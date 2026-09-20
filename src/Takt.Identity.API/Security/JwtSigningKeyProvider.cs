using System.Security.Cryptography;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Options;
using Takt.Identity.API.Configuration;

namespace Takt.Identity.API.Security;

public sealed class JwtSigningKeyProvider : IJwtSigningKeyProvider
{
    public JwtSigningKeyProvider(IOptions<TokenOptions> tokenOptions, IHostEnvironment hostEnvironment)
    {
        var options = tokenOptions.Value;
        var pem = ResolvePrivateKeyPem(options, hostEnvironment);

        var rsa = RSA.Create();
        rsa.ImportFromPem(pem);

        var rsaKey = new RsaSecurityKey(rsa)
        {
            KeyId = string.IsNullOrWhiteSpace(options.KeyId)
                ? Base64UrlEncoder.Encode(SHA256.HashData(rsa.ExportSubjectPublicKeyInfo()))
                : options.KeyId
        };

        SigningKey = rsaKey;
        SigningCredentials = new SigningCredentials(SigningKey, SecurityAlgorithms.RsaSha256);

        var publicParameters = rsa.ExportParameters(false);
        JsonWebKey = new JsonWebKey
        {
            Kty = JsonWebAlgorithmsKeyTypes.RSA,
            Use = JsonWebKeyUseNames.Sig,
            Alg = SecurityAlgorithms.RsaSha256,
            Kid = rsaKey.KeyId,
            N = Base64UrlEncoder.Encode(publicParameters.Modulus!),
            E = Base64UrlEncoder.Encode(publicParameters.Exponent!)
        };
    }

    public SigningCredentials SigningCredentials { get; }
    public SecurityKey SigningKey { get; }
    public JsonWebKey JsonWebKey { get; }

    private static string ResolvePrivateKeyPem(TokenOptions options, IHostEnvironment hostEnvironment)
    {
        if (!string.IsNullOrWhiteSpace(options.PrivateKeyPem))
        {
            return options.PrivateKeyPem;
        }

        if (!string.IsNullOrWhiteSpace(options.PrivateKeyPemPath))
        {
            if (!hostEnvironment.IsDevelopment())
            {
                throw new InvalidOperationException(
                    "Tokens:PrivateKeyPemPath is Development-only. In non-development environments, configure Tokens:PrivateKeyPem via secure secrets.");
            }

            return File.ReadAllText(options.PrivateKeyPemPath);
        }

        throw new InvalidOperationException(
            "JWT signing key is not configured. Set Tokens:PrivateKeyPemPath or Tokens:PrivateKeyPem.");
    }
}
