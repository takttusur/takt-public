using Microsoft.EntityFrameworkCore;
using Takt.People.API.Persistence;

namespace Takt.People.API.Services;

public sealed class PeopleService(
    PeopleAppDbContext dbContext,
    IPersonAnonymizationService anonymizationService) : IPeopleService
{
    public async Task<PersonView?> GetByIdAsync(long id, CancellationToken cancellationToken)
    {
        var person = await dbContext.People
            .AsNoTracking()
            .Include(x => x.PersonalDataConsent)
            .Include(x => x.Memberships)
                .ThenInclude(x => x.Branch)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return person is null ? null : ToView(person);
    }

    public async Task<IReadOnlyList<PersonView>> SearchAsync(SearchPeopleQuery query, CancellationToken cancellationToken)
    {
        var people = dbContext.People
            .AsNoTracking()
            .Include(x => x.PersonalDataConsent)
            .Include(x => x.Memberships)
                .ThenInclude(x => x.Branch)
            .AsSplitQuery()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Query))
        {
            var search = query.Query.Trim();
            people = people.Where(x =>
                EF.Functions.ILike(x.FirstName, $"%{search}%") ||
                EF.Functions.ILike(x.LastName, $"%{search}%") ||
                EF.Functions.ILike(x.Alias, $"%{search}%") ||
                (x.FathersName != null && EF.Functions.ILike(x.FathersName, $"%{search}%")));
        }

        if (!string.IsNullOrWhiteSpace(query.Alias))
        {
            people = people.Where(x => x.Alias == query.Alias);
        }

        if (query.BirthYear.HasValue)
        {
            people = people.Where(x => x.BirthYear == query.BirthYear);
        }

        if (query.BirthMonth.HasValue)
        {
            people = people.Where(x => x.BirthMonth == query.BirthMonth);
        }

        if (query.BirthDay.HasValue)
        {
            people = people.Where(x => x.BirthDay == query.BirthDay);
        }

        var result = await people
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ThenBy(x => x.Id)
            .Skip(query.Offset)
            .Take(query.Limit)
            .ToListAsync(cancellationToken);

        return result.Select(ToView).ToArray();
    }

    public async Task<PersonView> CreateAsync(PersonData personData, CancellationToken cancellationToken)
    {
        ValidatePersonData(personData);
        var now = DateTimeOffset.UtcNow;
        var entity = new Person
        {
            FirstName = personData.FirstName.Trim(),
            LastName = personData.LastName.Trim(),
            FathersName = TrimOptional(personData.FathersName),
            Alias = personData.Alias.Trim(),
            BirthYear = personData.BirthYear,
            BirthMonth = personData.BirthMonth,
            BirthDay = personData.BirthDay,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.People.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return ToView(entity);
    }

    public Task<PersonView?> UpdateAsPersonAsync(long id, PersonData personData, CancellationToken cancellationToken)
    {
        return UpdateInternalAsync(id, personData, cancellationToken);
    }

    public Task<PersonView?> UpdateAsAdminAsync(long id, PersonData personData, CancellationToken cancellationToken)
    {
        return UpdateInternalAsync(id, personData, cancellationToken);
    }

    public async Task<PersonView> AcceptPersonalDataConsentAsync(AcceptConsentCommand command, CancellationToken cancellationToken)
    {
        ValidatePersonData(command.PersonData);
        if (string.IsNullOrWhiteSpace(command.ConsentVersion))
        {
            throw new ArgumentException("Consent version is required.", nameof(command.ConsentVersion));
        }

        var person = command.PersonId.HasValue
            ? await dbContext.People.Include(x => x.PersonalDataConsent)
                .Include(x => x.Memberships)
                    .ThenInclude(x => x.Branch)
                .SingleOrDefaultAsync(x => x.Id == command.PersonId.Value, cancellationToken)
            : null;

        if (person is null)
        {
            var identityHash = anonymizationService.BuildIdentityHash(command.PersonData);
            person = await dbContext.People
                .Include(x => x.PersonalDataConsent)
                .Include(x => x.Memberships)
                    .ThenInclude(x => x.Branch)
                .SingleOrDefaultAsync(x => x.IsAnonymized && x.AnonymizationKeyHash == identityHash, cancellationToken);
        }

        var now = DateTimeOffset.UtcNow;
        if (person is null)
        {
            person = new Person
            {
                FirstName = command.PersonData.FirstName.Trim(),
                LastName = command.PersonData.LastName.Trim(),
                FathersName = TrimOptional(command.PersonData.FathersName),
                Alias = command.PersonData.Alias.Trim(),
                BirthYear = command.PersonData.BirthYear,
                BirthMonth = command.PersonData.BirthMonth,
                BirthDay = command.PersonData.BirthDay,
                IsAnonymized = false,
                AnonymizationKeyHash = null,
                AnonymizedAt = null,
                CreatedAt = now,
                UpdatedAt = now
            };
            dbContext.People.Add(person);
        }
        else
        {
            person.FirstName = command.PersonData.FirstName.Trim();
            person.LastName = command.PersonData.LastName.Trim();
            person.FathersName = TrimOptional(command.PersonData.FathersName);
            person.Alias = command.PersonData.Alias.Trim();
            person.BirthYear = command.PersonData.BirthYear;
            person.BirthMonth = command.PersonData.BirthMonth;
            person.BirthDay = command.PersonData.BirthDay;
            person.IsAnonymized = false;
            person.AnonymizationKeyHash = null;
            person.AnonymizedAt = null;
            person.UpdatedAt = now;
        }

        if (person.PersonalDataConsent is null)
        {
            person.PersonalDataConsent = new PersonalDataConsent
            {
                Version = command.ConsentVersion.Trim(),
                AcceptedAt = command.AcceptedAt,
                AcceptedFromIp = TrimOptional(command.AcceptedFromIp)
            };
        }
        else
        {
            person.PersonalDataConsent.Version = command.ConsentVersion.Trim();
            person.PersonalDataConsent.AcceptedAt = command.AcceptedAt;
            person.PersonalDataConsent.AcceptedFromIp = TrimOptional(command.AcceptedFromIp);
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToView(person);
    }

    public async Task<bool> AnonymizeAsync(long id, CancellationToken cancellationToken)
    {
        var person = await dbContext.People
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (person is null)
        {
            return false;
        }

        var now = DateTimeOffset.UtcNow;
        var source = new PersonData(
            person.FirstName,
            person.LastName,
            person.FathersName,
            person.Alias,
            person.BirthYear,
            person.BirthMonth,
            person.BirthDay);
        person.AnonymizationKeyHash = anonymizationService.BuildIdentityHash(source);
        person.FirstName = "Anonymized";
        person.LastName = "Person";
        person.FathersName = null;
        person.Alias = $"anon-{person.Id}";
        person.IsAnonymized = true;
        person.AnonymizedAt = now;
        person.UpdatedAt = now;

        await dbContext.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<IReadOnlyList<ClubRoleView>> GetRolesAsync(CancellationToken cancellationToken)
    {
        return await Task.FromResult(ClubRoleCatalog.GetRoles());
    }

    public async Task<IReadOnlyList<BranchView>> GetBranchesAsync(CancellationToken cancellationToken)
    {
        return await dbContext.Branches
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .Select(x => new BranchView(x.Id, x.Name, x.Code, x.CreatedAt, x.UpdatedAt))
            .ToListAsync(cancellationToken);
    }

    public async Task<BranchView> CreateBranchAsync(CreateBranchCommand command, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(command.Name))
        {
            throw new ArgumentException("Branch name is required.", nameof(command.Name));
        }

        var now = DateTimeOffset.UtcNow;
        var branch = new Branch
        {
            Name = command.Name.Trim(),
            Code = TrimOptional(command.Code),
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.Branches.Add(branch);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new BranchView(branch.Id, branch.Name, branch.Code, branch.CreatedAt, branch.UpdatedAt);
    }

    public async Task<ClubMembershipView> AddMembershipAsync(ClubMembershipData membershipData, CancellationToken cancellationToken)
    {
        ValidateMembershipDates(membershipData.StartDate, membershipData.FinishDate);

        var personExists = await dbContext.People
            .AsNoTracking()
            .AnyAsync(x => x.Id == membershipData.PersonId, cancellationToken);
        if (!personExists)
        {
            throw new InvalidOperationException("Person does not exist.");
        }

        if (!ClubRoleCatalog.TryParse(membershipData.RoleId, out var role))
        {
            throw new InvalidOperationException("Role does not exist.");
        }

        Branch? branch = null;
        if (membershipData.BranchId.HasValue)
        {
            branch = await dbContext.Branches
                .AsNoTracking()
                .SingleOrDefaultAsync(x => x.Id == membershipData.BranchId.Value, cancellationToken)
                ?? throw new InvalidOperationException("Branch does not exist.");
        }

        ValidateRoleScope(role, membershipData.BranchId);

        var now = DateTimeOffset.UtcNow;
        var entity = new ClubMembership
        {
            PersonId = membershipData.PersonId,
            RoleId = role,
            BranchId = membershipData.BranchId,
            StartDate = membershipData.StartDate,
            FinishDate = membershipData.FinishDate,
            CreatedAt = now,
            UpdatedAt = now
        };

        dbContext.ClubMemberships.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return new ClubMembershipView(
            entity.Id,
            entity.PersonId,
            (int)entity.RoleId,
            ClubRoleCatalog.Get(entity.RoleId).Name,
            entity.BranchId,
            branch?.Name,
            entity.StartDate,
            entity.FinishDate,
            entity.CreatedAt,
            entity.UpdatedAt);
    }

    public async Task<IReadOnlyList<ClubMembershipView>> SearchMembershipsAsync(SearchMembershipsQuery query, CancellationToken cancellationToken)
    {
        var memberships = dbContext.ClubMemberships
            .AsNoTracking()
            .Include(x => x.Branch)
            .AsQueryable();

        if (query.PersonId.HasValue)
        {
            memberships = memberships.Where(x => x.PersonId == query.PersonId.Value);
        }

        if (query.BranchId.HasValue)
        {
            memberships = memberships.Where(x => x.BranchId == query.BranchId.Value);
        }

        if (query.RoleId.HasValue)
        {
            if (!ClubRoleCatalog.TryParse(query.RoleId.Value, out var role))
            {
                return [];
            }

            memberships = memberships.Where(x => x.RoleId == role);
        }

        if (query.ActiveOn.HasValue)
        {
            var activeOn = query.ActiveOn.Value;
            memberships = memberships.Where(x => x.StartDate <= activeOn && (x.FinishDate == null || x.FinishDate >= activeOn));
        }

        var result = await memberships
            .OrderBy(x => x.PersonId)
            .ThenBy(x => x.StartDate)
            .ThenBy(x => x.Id)
            .Skip(query.Offset)
            .Take(query.Limit)
            .ToListAsync(cancellationToken);

        return result.Select(ToMembershipView).ToArray();
    }

    private async Task<PersonView?> UpdateInternalAsync(long id, PersonData personData, CancellationToken cancellationToken)
    {
        ValidatePersonData(personData);

        var person = await dbContext.People
            .Include(x => x.PersonalDataConsent)
            .Include(x => x.Memberships)
                .ThenInclude(x => x.Branch)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
        if (person is null)
        {
            return null;
        }

        person.FirstName = personData.FirstName.Trim();
        person.LastName = personData.LastName.Trim();
        person.FathersName = TrimOptional(personData.FathersName);
        person.Alias = personData.Alias.Trim();
        person.BirthYear = personData.BirthYear;
        person.BirthMonth = personData.BirthMonth;
        person.BirthDay = personData.BirthDay;
        person.UpdatedAt = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);
        return ToView(person);
    }

    private static PersonView ToView(Person person)
    {
        var consent = person.PersonalDataConsent is null
            ? null
            : new PersonalDataConsentData(
                person.PersonalDataConsent.Version,
                person.PersonalDataConsent.AcceptedAt,
                person.PersonalDataConsent.AcceptedFromIp);

        return new PersonView(
            person.Id,
            person.FirstName,
            person.LastName,
            person.FathersName,
            person.Alias,
            person.BirthYear,
            person.BirthMonth,
            person.BirthDay,
            person.IsAnonymized,
            person.CreatedAt,
            person.UpdatedAt,
            consent,
            person.Memberships.OrderBy(x => x.StartDate).ThenBy(x => x.Id).Select(ToMembershipView).ToArray());
    }

    private static ClubMembershipView ToMembershipView(ClubMembership membership)
    {
        return new ClubMembershipView(
            membership.Id,
            membership.PersonId,
            (int)membership.RoleId,
            ClubRoleCatalog.Get(membership.RoleId).Name,
            membership.BranchId,
            membership.Branch?.Name,
            membership.StartDate,
            membership.FinishDate,
            membership.CreatedAt,
            membership.UpdatedAt);
    }

    private static void ValidatePersonData(PersonData personData)
    {
        if (string.IsNullOrWhiteSpace(personData.FirstName))
        {
            throw new ArgumentException("First name is required.", nameof(personData.FirstName));
        }

        if (string.IsNullOrWhiteSpace(personData.LastName))
        {
            throw new ArgumentException("Last name is required.", nameof(personData.LastName));
        }

        if (string.IsNullOrWhiteSpace(personData.Alias))
        {
            throw new ArgumentException("Alias is required.", nameof(personData.Alias));
        }

        if (personData.BirthDay.HasValue && !personData.BirthMonth.HasValue)
        {
            throw new ArgumentException("Birth month is required when birth day is provided.", nameof(personData.BirthMonth));
        }

        if (personData.BirthMonth.HasValue && !personData.BirthYear.HasValue)
        {
            throw new ArgumentException("Birth year is required when birth month is provided.", nameof(personData.BirthYear));
        }

        if (personData.BirthYear is < 1 or > 9999)
        {
            throw new ArgumentException("Birth year must be between 1 and 9999.", nameof(personData.BirthYear));
        }

        if (personData.BirthMonth is < 1 or > 12)
        {
            throw new ArgumentException("Birth month must be between 1 and 12.", nameof(personData.BirthMonth));
        }

        if (personData.BirthDay is < 1 or > 31)
        {
            throw new ArgumentException("Birth day must be between 1 and 31.", nameof(personData.BirthDay));
        }

        if (personData.BirthYear.HasValue && personData.BirthMonth.HasValue && personData.BirthDay.HasValue)
        {
            _ = new DateOnly(personData.BirthYear.Value, personData.BirthMonth.Value, personData.BirthDay.Value);
        }
    }

    private static string? TrimOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void ValidateMembershipDates(DateOnly startDate, DateOnly? finishDate)
    {
        if (finishDate.HasValue && finishDate.Value < startDate)
        {
            throw new ArgumentException("Finish date cannot be earlier than start date.", nameof(finishDate));
        }
    }

    private static void ValidateRoleScope(ClubRoleId role, long? branchId)
    {
        var roleInfo = ClubRoleCatalog.Get(role);
        if (roleInfo.RequiresBranch && !branchId.HasValue)
        {
            throw new ArgumentException($"Role '{roleInfo.Name}' requires branchId.", nameof(branchId));
        }

        if (!roleInfo.AllowsBranch && branchId.HasValue)
        {
            throw new ArgumentException($"Role '{roleInfo.Name}' must be global and cannot contain branchId.", nameof(branchId));
        }
    }
}
