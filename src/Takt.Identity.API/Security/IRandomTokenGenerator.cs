namespace Takt.Identity.API.Security;

public interface IRandomTokenGenerator
{
    string Create(int byteLength = 32);
}
