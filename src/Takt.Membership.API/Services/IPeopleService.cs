namespace Takt.People.API.Services;

public interface IPeopleService
{
    Task<PersonView?> GetByIdAsync(long id, CancellationToken cancellationToken);
    Task<IReadOnlyList<PersonView>> SearchAsync(SearchPeopleQuery query, CancellationToken cancellationToken);
    Task<PersonView> CreateAsync(PersonData personData, CancellationToken cancellationToken);
    Task<PersonView?> UpdateAsPersonAsync(long id, PersonData personData, CancellationToken cancellationToken);
    Task<PersonView?> UpdateAsAdminAsync(long id, PersonData personData, CancellationToken cancellationToken);
    Task<PersonView> AcceptPersonalDataConsentAsync(AcceptConsentCommand command, CancellationToken cancellationToken);
    Task<bool> AnonymizeAsync(long id, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClubRoleView>> GetRolesAsync(CancellationToken cancellationToken);
    Task<IReadOnlyList<BranchView>> GetBranchesAsync(CancellationToken cancellationToken);
    Task<BranchView> CreateBranchAsync(CreateBranchCommand command, CancellationToken cancellationToken);
    Task<ClubMembershipView> AddMembershipAsync(ClubMembershipData membershipData, CancellationToken cancellationToken);
    Task<IReadOnlyList<ClubMembershipView>> SearchMembershipsAsync(SearchMembershipsQuery query, CancellationToken cancellationToken);
}
