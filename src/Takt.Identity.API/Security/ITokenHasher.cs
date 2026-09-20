namespace Takt.Identity.API.Security;

public interface ITokenHasher
{
    string HashToken(string token);
}
