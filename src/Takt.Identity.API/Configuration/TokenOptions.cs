namespace Takt.Identity.API.Configuration;

public sealed class TokenOptions
{
    public const string SectionName = "Tokens";
    public string Issuer { get; set; } = "takt.identity";
    public string Audience { get; set; } = "takt.api";
    public int AccessTokenLifetimeMinutes { get; set; } = 15;
    public int RefreshTokenLifetimeDays { get; set; } = 30;
    public string? PrivateKeyPemPath { get; set; }
    public string? PrivateKeyPem { get; set; }
    public string? KeyId { get; set; }
}
