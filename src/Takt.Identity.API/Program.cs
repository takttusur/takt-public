using System.Security.Claims;
using System.Threading.RateLimiting;
using Asp.Versioning;
using Asp.Versioning.Builder;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Scalar.AspNetCore;
using Takt.Identity.API.Bootstrap;
using AppTokenOptions = Takt.Identity.API.Configuration.TokenOptions;
using Takt.Identity.API.Configuration;
using Takt.Identity.API.Controllers;
using Takt.Identity.API.Constants;
using Takt.Identity.API.OpenApi;
using Takt.Identity.API.Persistence;
using Takt.Identity.API.Services;

namespace Takt.Identity.API;

public sealed class Program
{
    public static async Task Main(string[] args)
    {
        var migrateOnly = args.Contains("--migrate-db", StringComparer.OrdinalIgnoreCase);
        var bootstrapAdminOnly =
            args.Contains("--seed", StringComparer.OrdinalIgnoreCase) ||
            args.Contains("--bootstrap-admin", StringComparer.OrdinalIgnoreCase);

        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddProblemDetails();
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition(JwtBearerDefaults.AuthenticationScheme, new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "JWT Authorization header using the Bearer scheme. Example: \"Bearer {token}\"",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT"
            });
            options.OperationFilter<AuthorizeOperationFilter>();
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

        builder.Services.Configure<InvitationOptions>(builder.Configuration.GetSection(InvitationOptions.SectionName));
        builder.Services.Configure<BootstrapOptions>(builder.Configuration.GetSection(BootstrapOptions.SectionName));
        builder.Services.Configure<AppTokenOptions>(builder.Configuration.GetSection(AppTokenOptions.SectionName));
        builder.Services.Configure<PasskeyOptions>(builder.Configuration.GetSection(PasskeyOptions.SectionName));
        builder.Services.Configure<SecurityOptions>(builder.Configuration.GetSection(SecurityOptions.SectionName));
        builder.Services.Configure<DatabaseOptions>(builder.Configuration.GetSection(DatabaseOptions.SectionName));
        builder.Services.Configure<NetworkOptions>(builder.Configuration.GetSection(NetworkOptions.SectionName));
        builder.Services.Configure<IdentityPasskeyOptions>(builder.Configuration.GetSection(PasskeyOptions.SectionName));
        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders =
                ForwardedHeaders.XForwardedFor |
                ForwardedHeaders.XForwardedProto |
                ForwardedHeaders.XForwardedHost;
            options.KnownIPNetworks.Clear();
            options.KnownProxies.Clear();
        });

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
                options.Password.RequiredLength = 8;
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
            .AddCheck("self", () => HealthCheckResult.Healthy(), tags: ["live"])
            .AddNpgSql(
                connectionString,
                name: "postgresql",
                tags: ["ready"]);

        var app = builder.Build();
        var networkOptions = app.Services.GetRequiredService<IOptions<NetworkOptions>>().Value;

        app.UseForwardedHeaders();
        app.Use((context, next) =>
        {
            const string forwardedPrefixHeader = "X-Forwarded-Prefix";
            if (context.Request.Headers.TryGetValue(forwardedPrefixHeader, out var prefixValues))
            {
                var prefix = prefixValues.ToString()
                    .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                    .LastOrDefault();
                if (!string.IsNullOrWhiteSpace(prefix))
                {
                    context.Request.PathBase = new PathString(prefix);
                }
            }

            return next();
        });
        app.UseExceptionHandler();
        app.UseCors("default");
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        if (networkOptions.HttpsRedirectionEnabled)
        {
            app.UseHttpsRedirection();
        }

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true
        });
        app.MapHealthChecks("/health/live", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("live")
        });
        app.MapHealthChecks("/health/ready", new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("ready")
        });
        
        app.MapScalarApiReference("/scalar", (options, httpContext) =>
        {
            var serverPathBase = ResolveExternalBaseUrl(httpContext.Request, networkOptions);
            var pathBase = httpContext.Request.PathBase.Value;
            options.WithOpenApiRoutePattern(string.IsNullOrWhiteSpace(pathBase)
                ? "/openapi/{documentName}.json"
                : $"{pathBase}/openapi/{{documentName}}.json");
            options.WithDynamicBaseServerUrl(false);
            options.Servers = [new ScalarServer(serverPathBase, "Identity API")];
            options.WithDefaultHttpClient(ScalarTarget.CSharp, ScalarClient.HttpClient);
        });

        app.UseSwagger(options =>
        {
            options.RouteTemplate = "openapi/{documentName}.json";
            options.PreSerializeFilters.Add((document, request) =>
            {
                var serverPathBase = ResolveExternalBaseUrl(request, networkOptions);
                document.Servers = [new OpenApiServer { Url = serverPathBase }];
            });
        });

        app.MapControllers();

        using (var scope = app.Services.CreateScope())
        {
            var dbOptions = scope.ServiceProvider.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            if (migrateOnly)
            {
                var db = scope.ServiceProvider.GetRequiredService<IdentityAppDbContext>();
                await db.Database.MigrateAsync();
            }

            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<long>>>();
            await EnsureRolesAsync(roleManager);
        }

        if (migrateOnly)
        {
            return;
        }

        if (bootstrapAdminOnly)
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

    private static string BuildExternalBaseUrl(HttpRequest request)
    {
        var forwardedScheme = request.Headers["X-Forwarded-Proto"].ToString();
        var scheme = string.IsNullOrWhiteSpace(forwardedScheme)
            ? request.Scheme
            : forwardedScheme.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? request.Scheme;

        var forwardedHost = request.Headers["X-Forwarded-Host"].ToString();
        var host = string.IsNullOrWhiteSpace(forwardedHost)
            ? request.Host.Value
            : forwardedHost.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? request.Host.Value;

        var forwardedPrefix = request.Headers["X-Forwarded-Prefix"].ToString();
        var prefix = string.IsNullOrWhiteSpace(forwardedPrefix)
            ? request.PathBase.Value ?? string.Empty
            : forwardedPrefix.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries).LastOrDefault() ?? string.Empty;

        if (!string.IsNullOrWhiteSpace(prefix) && !prefix.StartsWith('/'))
        {
            prefix = $"/{prefix}";
        }
        prefix = prefix.TrimEnd('/');

        return string.IsNullOrWhiteSpace(prefix)
            ? $"{scheme}://{host}"
            : $"{scheme}://{host}{prefix}";
    }

    private static string ResolveExternalBaseUrl(HttpRequest request, NetworkOptions networkOptions)
    {
        if (!string.IsNullOrWhiteSpace(networkOptions.BaseUrl))
        {
            if (!Uri.TryCreate(networkOptions.BaseUrl, UriKind.Absolute, out var configuredBaseUrl))
            {
                throw new InvalidOperationException("Network:BaseUrl must be an absolute URL.");
            }

            return configuredBaseUrl.ToString().TrimEnd('/');
        }

        return BuildExternalBaseUrl(request);
    }
}
