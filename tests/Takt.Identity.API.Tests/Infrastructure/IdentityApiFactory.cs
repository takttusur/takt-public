using System.Security.Cryptography;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Takt.Identity.API.Persistence;

namespace Takt.Identity.API.Tests.Infrastructure;

public sealed class IdentityApiFactory(
    string connectionString,
    IReadOnlyDictionary<string, string?>? configurationOverrides = null) : WebApplicationFactory<Program>
{
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            var key = RSA.Create(2048).ExportRSAPrivateKeyPem();
            var settings = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString,
                ["Tokens:Issuer"] = "https://identity.test",
                ["Tokens:Audience"] = "takt-test",
                ["Tokens:PrivateKeyPem"] = key,
                ["Passkeys:ServerDomain"] = "localhost",
                ["Passkeys:AllowedOrigins:0"] = "http://localhost",
                ["Cors:AllowedOrigins:0"] = "http://localhost",
                ["Network:HttpsRedirectionEnabled"] = "false",
                ["Database:ApplyMigrationsOnStartup"] = "true"
            };

            if (configurationOverrides is not null)
            {
                foreach (var pair in configurationOverrides)
                {
                    settings[pair.Key] = pair.Value;
                }
            }

            configBuilder.AddInMemoryCollection(settings);
        });

        var host = base.CreateHost(builder);
        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<IdentityAppDbContext>();
        db.Database.Migrate();
        return host;
    }
}
