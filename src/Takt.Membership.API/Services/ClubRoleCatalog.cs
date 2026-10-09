using Takt.People.API.Persistence;

namespace Takt.People.API.Services;

public static class ClubRoleCatalog
{
    private static readonly ClubRoleView[] Roles =
    [
        new((int)ClubRoleId.Guest, "Guest", false, false),
        new((int)ClubRoleId.BranchMember, "Branch Member", true, true),
        new((int)ClubRoleId.BranchLead, "Branch Lead", true, true),
        new((int)ClubRoleId.WarehouseLead, "Warehouse Lead", true, true),
        new((int)ClubRoleId.ClubPresident, "Club President", false, false)
    ];

    public static IReadOnlyList<ClubRoleView> GetRoles() => Roles;

    public static bool TryParse(int roleId, out ClubRoleId role)
    {
        if (Enum.IsDefined(typeof(ClubRoleId), roleId))
        {
            role = (ClubRoleId)roleId;
            return true;
        }

        role = default;
        return false;
    }

    public static ClubRoleView Get(ClubRoleId roleId)
    {
        return Roles.Single(x => x.Id == (int)roleId);
    }
}
