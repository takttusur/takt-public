namespace Takt.Identity.API.Configuration;

public sealed class DatabaseOptions
{
    public const string SectionName = "Database";
    
    /// <summary>
    /// This parameter should be used only for tests. Not in Production.
    /// </summary>
    public bool ApplyMigrationsOnStartup { get; set; }
}
