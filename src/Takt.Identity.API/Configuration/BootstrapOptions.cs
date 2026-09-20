namespace Takt.Identity.API.Configuration;

public sealed class BootstrapOptions
{
    public const string SectionName = "Bootstrap";
    public string AdminUserName { get; set; } = "admin";
}
