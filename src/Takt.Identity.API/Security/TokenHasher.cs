using System.Security.Cryptography;
using System.Text;

namespace Takt.Identity.API.Security;

public sealed class TokenHasher : ITokenHasher
{
    public string HashToken(string token)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(bytes);
    }
}
