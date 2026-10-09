using System.Security.Cryptography;
using System.Text;

namespace Takt.People.API.Services;

public sealed class PersonAnonymizationService : IPersonAnonymizationService
{
    public string BuildIdentityHash(PersonData personData)
    {
        var normalized =
            $"{Normalize(personData.LastName)}|{Normalize(personData.FirstName)}|{Normalize(personData.FathersName)}|" +
            $"{personData.BirthYear?.ToString() ?? "_"}|{personData.BirthMonth?.ToString() ?? "_"}|{personData.BirthDay?.ToString() ?? "_"}";
        return Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(normalized)));
    }

    private static string Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? "_" : value.Trim().ToUpperInvariant();
}
