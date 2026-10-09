namespace Takt.Warehouse.API.Configuration;

public sealed class NetworkOptions
{
    public const string SectionName = "Network";

    public bool HttpsRedirectionEnabled { get; init; }
}
