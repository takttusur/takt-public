namespace Takt.People.API.Persistence;

public sealed class Person
{
    public long Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? FathersName { get; set; }
    public string Alias { get; set; } = string.Empty;
    public int? BirthYear { get; set; }
    public int? BirthMonth { get; set; }
    public int? BirthDay { get; set; }
    public bool IsAnonymized { get; set; }
    public string? AnonymizationKeyHash { get; set; }
    public DateTimeOffset? AnonymizedAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public PersonalDataConsent? PersonalDataConsent { get; set; }
    public ICollection<ClubMembership> Memberships { get; set; } = [];
}
