namespace Takt.People.API.Persistence;

public sealed class ClubMembership
{
    public long Id { get; set; }
    public long PersonId { get; set; }
    public ClubRoleId RoleId { get; set; }
    public long? BranchId { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? FinishDate { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
    public Person Person { get; set; } = null!;
    public Branch? Branch { get; set; }
}
