namespace Takt.People.API.Persistence;

public sealed class PersonalDataConsent
{
    public long Id { get; set; }
    public long PersonId { get; set; }
    public string Version { get; set; } = string.Empty;
    public DateTimeOffset AcceptedAt { get; set; }
    public string? AcceptedFromIp { get; set; }

    public Person Person { get; set; } = null!;
}
