using Asp.Versioning;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Takt.People.API.Configuration;
using Takt.People.API.Constants;
using Takt.People.API.Persistence;
using Takt.People.API.Services;

namespace Takt.People.API;

public sealed class Program
{
    public static async Task Main(string[] args)
    {
        var migrateOnly = args.Contains("--migrate-db", StringComparer.OrdinalIgnoreCase);

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddProblemDetails();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "JWT Authorization header using the ****** Example: \"******\"",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });
        });
        builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc()
            .AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'VVV";
                options.SubstituteApiVersionInUrl = true;
            });

        builder.Services.Configure<NetworkOptions>(builder.Configuration.GetSection(NetworkOptions.SectionName));

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured.");
        builder.Services.AddDbContext<PeopleAppDbContext>(options => options.UseNpgsql(connectionString));

        builder.Services.AddScoped<IPeopleService, PeopleService>();
        builder.Services.AddSingleton<IPersonAnonymizationService, PersonAnonymizationService>();

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.AdminOnly, policy => policy.RequireRole(SystemRoles.Admin));
        });

        builder.Services.AddHealthChecks()
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
            .AddNpgSql(connectionString, name: "postgresql", tags: ["ready"]);

        var app = builder.Build();
        var networkOptions = app.Services.GetRequiredService<Microsoft.Extensions.Options.IOptions<NetworkOptions>>().Value;

        app.UseExceptionHandler();
        app.UseAuthentication();
        app.UseAuthorization();
        if (networkOptions.HttpsRedirectionEnabled)
        {
            app.UseHttpsRedirection();
        }

        app.MapHealthChecks("/health", new HealthCheckOptions { Predicate = _ => true });
        app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = check => check.Tags.Contains("live") });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

        app.MapScalarApiReference("/scalar");
        app.UseSwagger(options =>
        {
            options.RouteTemplate = "openapi/{documentName}.json";
        });

        app.MapControllers();

        if (migrateOnly)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<PeopleAppDbContext>();
            await db.Database.MigrateAsync();
            return;
        }

        await app.RunAsync();
    }
}