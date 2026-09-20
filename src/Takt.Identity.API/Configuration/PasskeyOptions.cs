namespace Takt.Identity.API.Configuration;

public sealed class PasskeyOptions
{
    public const string SectionName = "Passkeys";

    public string? ServerDomain { get; set; }
    public string ServerName { get; set; } = "Takt Identity";
    public string UserVerificationRequirement { get; set; } = "required";
    public string ResidentKeyRequirement { get; set; } = "preferred";
    public string[] AllowedOrigins { get; set; } = ["https://localhost"];
    public int ChallengeTtlMinutes { get; set; } = 5;
}
