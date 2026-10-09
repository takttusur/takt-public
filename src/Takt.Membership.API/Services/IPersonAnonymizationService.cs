namespace Takt.People.API.Services;

public interface IPersonAnonymizationService
{
    string BuildIdentityHash(PersonData personData);
}
