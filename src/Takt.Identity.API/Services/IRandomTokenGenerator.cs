namespace Takt.Identity.API.Services;

public interface IRandomTokenGenerator
{
    string Create(int byteLength = 32);
}
