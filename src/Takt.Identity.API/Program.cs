using System.Security.Claims;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Takt.Identity.API.Bootstrap;
using AppTokenOptions = Takt.Identity.API.Configuration.TokenOptions;
using Takt.Identity.API.Configuration;
using Takt.Identity.API.Controllers;
using Takt.Identity.API.Constants;
using Takt.Identity.API.Persistence;
using Takt.Identity.API.Security;

namespace Takt.Identity.API;

public sealed class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddProblemDetails();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddOpenApi();
        builder.Services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
            .AddMvc();

        builder.Services.Configure<InvitationOptions>(builder.Configuration.GetSection(InvitationOptions.SectionName));
        builder.Services.Configure<BootstrapOptions>(builder.Configuration.GetSection(BootstrapOptions.SectionName));
        builder.Services.Configure<AppTokenOptions>(builder.Configuration.GetSection(AppTokenOptions.SectionName));
        builder.Services.Configure<PasskeyOptions>(builder.Configuration.GetSection(PasskeyOptions.SectionName));
        builder.Services.Configure<SecurityOptions>(builder.Configuration.GetSection(SecurityOptions.SectionName));
        builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection(DatabaseOptions.SectionName));
        builder.Services.Configure<IdentityPasskeyOptions>(builder.Configuration.GetSection(PasskeyOptions.SectionName));

        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection must be configured.");

        builder.Services.AddDbContext<IdentityAppDbContext>(options => options.UseNpgsql(connectionString));

        builder.Services
            .AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = false;
                options.SignIn.RequireConfirmedAccount = false;
                options.SignIn.RequireConfirmedEmail = false;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 12;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;
                options.Tokens.AuthenticatorIssuer = "Takt.Identity";
            })
            .AddRoles<IdentityRole<long>>()
            .AddSignInManager()
            .AddEntityFrameworkStores<IdentityAppDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddScoped<IRandomTokenGenerator, RandomTokenGenerator>();
        builder.Services.AddScoped<ITokenHasher, TokenHasher>();
        builder.Services.AddScoped<IAuthFlowStateService, AuthFlowStateService>();
        builder.Services.AddSingleton<IJwtSigningKeyProvider, JwtSigningKeyProvider>();
        builder.Services.AddScoped<ITokenService, TokenService>();
        builder.Services.AddScoped<IAuditService, AuditService>();
        builder.Services.AddScoped<BootstrapService>();
        builder.Services.AddScoped<StrongAuthenticationPolicy>();
        builder.Services.AddSingleton(TimeProvider.System);

        builder.Services.AddRateLimiter(options =>
        {
            options.AddPolicy("auth", context =>
                RateLimitPartition.GetFixedWindowLimiter(
                    context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = 10,
                        Window = TimeSpan.FromMinutes(1),
                        QueueLimit = 0
                    }));
        });

        builder.Services.AddCors(options =>
        {
            var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
            options.AddPolicy("default", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        builder.Services.AddOptions<IdentityPasskeyOptions>()
            .Configure<IOptions<PasskeyOptions>>((options, passkeyOptionsAccessor) =>
            {
                var passkey = passkeyOptionsAccessor.Value;
                options.ServerDomain = passkey.ServerDomain;
                options.UserVerificationRequirement = passkey.UserVerificationRequirement;
                options.ResidentKeyRequirement = passkey.ResidentKeyRequirement;
                options.ValidateOrigin = context => ValueTask.FromResult(
                    passkey.AllowedOrigins.Any(allowed =>
                        string.Equals(allowed, context.Origin, StringComparison.OrdinalIgnoreCase)));
            });

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer();

        builder.Services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IJwtSigningKeyProvider, IOptions<AppTokenOptions>>((options, signingProvider, tokenOptionsAccessor) =>
            {
                var tokenOptions = tokenOptionsAccessor.Value;
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = signingProvider.SigningKey,
                    ValidateIssuer = true,
                    ValidIssuer = tokenOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = tokenOptions.Audience,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = "preferred_username",
                    RoleClaimType = ClaimTypes.Role
                };
            });

        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy(Policies.AdminOnly, policy => policy.RequireRole(SystemRoles.Admin));
        });

        builder.Services.AddHealthChecks()
            .AddNpgSql(connectionString);

        var app = builder.Build();

        app.UseExceptionHandler();
        app.UseCors("default");
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        if (!app.Environment.IsEnvironment("Testing"))
        {
            app.UseHttpsRedirection();
        }

        app.MapHealthChecks("/health");

        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI();

        app.MapControllers();

        using (var scope = app.Services.CreateScope())
        {
            var dbOptions = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            if (dbOptions.ApplyMigrationsOnStartup)
            {
                var db = scope.ServiceProvider.GetRequiredService<IdentityAppDbContext>();
                await db.Database.MigrateAsync();
            }

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<long>>>();
            await EnsureRolesAsync(roleManager);
        }

        if (args.Contains("--bootstrap-admin", StringComparer.OrdinalIgnoreCase))
        {
            using var scope = app.Services.CreateScope();
            var bootstrap = scope.ServiceProvider.GetRequiredService<BootstrapService>();
            await bootstrap.BootstrapInitialAdminAsync(CancellationToken.None);
            return;
        }

        await app.RunAsync();
    }

    private static async Task EnsureRolesAsync(RoleManager<IdentityRole<long>> roleManager)
    {
        foreach (var role in SystemRoles.All)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                var result = await roleManager.CreateAsync(new IdentityRole<long>(role));
                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Unable to create role '{role}': {errors}");
                }
            }
        }
    }
}
