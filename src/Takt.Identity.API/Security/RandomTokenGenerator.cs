using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;

namespace Takt.Identity.API.Security;

public sealed class RandomTokenGenerator : IRandomTokenGenerator
{
    public string Create(int byteLength = 32)
    {
        var bytes = RandomNumberGenerator.GetBytes(byteLength);
        return Base64UrlEncoder.Encode(bytes);
    }
}
