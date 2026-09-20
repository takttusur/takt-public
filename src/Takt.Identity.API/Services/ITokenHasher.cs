namespace Takt.Identity.API.Services;

public interface ITokenHasher
{
    string HashToken(string token);
}
