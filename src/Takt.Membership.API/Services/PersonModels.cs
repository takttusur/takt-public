namespace Takt.People.API.Services;

public sealed record PersonData(
    string FirstName,
    string LastName,
    string? FathersName,
    string Alias,
    int? BirthYear,
    int? BirthMonth,
    int? BirthDay);

public sealed record PersonalDataConsentData(
    string Version,
    DateTimeOffset AcceptedAt,
    string? AcceptedFromIp);

public sealed record PersonView(
    long Id,
    string FirstName,
    string LastName,
    string? FathersName,
    string Alias,
    int? BirthYear,
    int? BirthMonth,
    int? BirthDay,
    bool IsAnonymized,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt,
    PersonalDataConsentData? Consent,
    IReadOnlyList<ClubMembershipView> Memberships);

public sealed record SearchPeopleQuery(
    string? Query,
    string? Alias,
    int? BirthYear,
    int? BirthMonth,
    int? BirthDay,
    int Offset,
    int Limit);

public sealed record AcceptConsentCommand(
    long? PersonId,
    PersonData PersonData,
    string ConsentVersion,
    DateTimeOffset AcceptedAt,
    string? AcceptedFromIp);

public sealed record ClubRoleView(
    int Id,
    string Name,
    bool RequiresBranch,
    bool AllowsBranch);

public sealed record BranchView(
    long Id,
    string Name,
    string? Code,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record CreateBranchCommand(
    string Name,
    string? Code);

public sealed record ClubMembershipData(
    long PersonId,
    int RoleId,
    long? BranchId,
    DateOnly StartDate,
    DateOnly? FinishDate);

public sealed record ClubMembershipView(
    long Id,
    long PersonId,
    int RoleId,
    string RoleName,
    long? BranchId,
    string? BranchName,
    DateOnly StartDate,
    DateOnly? FinishDate,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt);

public sealed record SearchMembershipsQuery(
    long? PersonId,
    long? BranchId,
    int? RoleId,
    DateOnly? ActiveOn,
    int Offset,
    int Limit);
