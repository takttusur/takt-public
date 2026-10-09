namespace Takt.People.API.Persistence;

public sealed class Branch
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Code { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<ClubMembership> Memberships { get; set; } = [];
}
