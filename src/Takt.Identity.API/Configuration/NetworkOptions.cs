namespace Takt.Identity.API.Configuration;

public sealed class NetworkOptions
{
    public const string SectionName = "Network";
    public bool HttpsRedirectionEnabled { get; set; }
    public string? BaseUrl { get; set; }
}
