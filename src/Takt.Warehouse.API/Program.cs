using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using Scalar.AspNetCore;
using Takt.Warehouse.API.Configuration;
using Takt.Warehouse.API.Persistence;
using Takt.Warehouse.API.Services;
using Takt.Warehouse.API.Telemetry;

namespace Takt.Warehouse.API;

public sealed class Program
{
    public static async Task Main(string[] args)
    {
        var migrateOnly = args.Contains("--migrate-db", StringComparer.OrdinalIgnoreCase);
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<NetworkOptions>(builder.Configuration.GetSection(NetworkOptions.SectionName));
        builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.SectionName));

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured.");

        builder.Services.AddDbContext<WarehouseDbContext>(options => options.UseNpgsql(connectionString));
        builder.Services.AddProblemDetails();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new() { Title = "Takt Warehouse API", Version = "v1" });
        });

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();
        builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IOptions<JwtOptions>>((options, jwtOptionsAccessor) =>
            {
                var jwtOptions = jwtOptionsAccessor.Value;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = jwtOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = jwtOptions.CreateSigningKey(),
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30)
                };
            });

        builder.Services.AddAuthorization();
        builder.Services.AddScoped<IInventoryService, InventoryService>();
        builder.Services.AddSingleton<IInventoryMetrics, InventoryMetrics>();
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
            .AddNpgSql(connectionString, name: "postgresql", tags: ["ready"]);

        builder.Services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService("Takt.Warehouse.API"))
            .WithTracing(tracing => tracing
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation()
                .AddSource("Takt.Warehouse.API"))
            .WithMetrics(metrics => metrics
                .AddAspNetCoreInstrumentation()
                .AddRuntimeInstrumentation()
                .AddMeter(InventoryMetrics.MeterName));

        var app = builder.Build();

        app.UseExceptionHandler();
        app.UseAuthentication();
        app.UseAuthorization();

        var networkOptions = app.Services.GetRequiredService<IOptions<NetworkOptions>>().Value;
        if (networkOptions.HttpsRedirectionEnabled)
        {
            app.UseHttpsRedirection();
        }

        app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => true });
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live")
        });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });

        app.MapScalarApiReference("/scalar");
        app.MapOpenApi();
        app.UseSwagger(options => options.RouteTemplate = "openapi/{documentName}.json");
        app.MapControllers();

        if (migrateOnly)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<WarehouseDbContext>();
            await db.Database.MigrateAsync();
            return;
        }

        await app.RunAsync();
    }
}