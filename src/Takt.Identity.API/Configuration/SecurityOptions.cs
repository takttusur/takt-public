namespace Takt.Identity.API.Configuration;

public sealed class SecurityOptions
{
    public const string SectionName = "Security";
    public int AuthenticationChallengeTtlMinutes { get; set; } = 10;
    public int StrongAuthenticationFreshnessMinutes { get; set; } = 10;
}
