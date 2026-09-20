namespace Takt.Identity.API.Configuration;

public sealed class InvitationOptions
{
    public const string SectionName = "Invitations";
    public int LifetimeHours { get; set; } = 24;
    public string? ActivationBaseUrl { get; set; }
}
