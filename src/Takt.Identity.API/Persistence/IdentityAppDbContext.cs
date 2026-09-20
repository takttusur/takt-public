using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Text.Json;

namespace Takt.Identity.API.Persistence;

public sealed class IdentityAppDbContext
    : IdentityDbContext<
        ApplicationUser,
        IdentityRole<long>,
        long,
        IdentityUserClaim<long>,
        IdentityUserRole<long>,
        IdentityUserLogin<long>,
        IdentityRoleClaim<long>,
        IdentityUserToken<long>,
        IdentityUserPasskey<long>>
{
    public IdentityAppDbContext(DbContextOptions<IdentityAppDbContext> options) : base(options)
    {
    }

    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<RefreshSession> RefreshSessions => Set<RefreshSession>();
    public DbSet<AuthFlowState> AuthFlowStates => Set<AuthFlowState>();
    public DbSet<AuditEvent> AuditEvents => Set<AuditEvent>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
            entity.Ignore(x => x.Email);
            entity.Ignore(x => x.NormalizedEmail);
            entity.Ignore(x => x.EmailConfirmed);
            entity.Ignore(x => x.PhoneNumber);
            entity.Ignore(x => x.PhoneNumberConfirmed);
            entity.Property(x => x.UserName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.NormalizedUserName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
            entity.HasIndex(x => x.NormalizedUserName).IsUnique();
        });

        builder.Entity<IdentityRole<long>>().ToTable("Roles");
        builder.Entity<IdentityUserRole<long>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<long>>().ToTable("UserClaims");
        builder.Entity<IdentityRoleClaim<long>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserLogin<long>>().ToTable("UserLogins");
        builder.Entity<IdentityUserToken<long>>().ToTable("UserTokens");
        builder.Entity<IdentityUserPasskey<long>>(entity =>
        {
            entity.ToTable("UserPasskeys");
            entity.HasKey(x => new { x.UserId, x.CredentialId });
            entity.Property(x => x.CredentialId).HasColumnType("bytea");
            entity.Property(x => x.Data)
                .HasConversion(PasskeyDataConverter, PasskeyDataComparer)
                .HasColumnType("jsonb");
        });

        builder.Entity<Invitation>(entity =>
        {
            entity.ToTable("Invitations");
            entity.Property(x => x.UserName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.NormalizedUserName).HasMaxLength(256).IsRequired();
            entity.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Role).HasMaxLength(32).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ExpiresAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ConsumedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.RevokedAt).HasColumnType("timestamp with time zone");
            entity.HasIndex(x => x.TokenHash).IsUnique();
        });

        builder.Entity<RefreshSession>(entity =>
        {
            entity.ToTable("RefreshSessions");
            entity.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ExpiresAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.RevokedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.RevokeReason).HasMaxLength(128);
            entity.HasIndex(x => x.TokenHash).IsUnique();
            entity.HasIndex(x => new { x.UserId, x.RevokedAt, x.ExpiresAt });
        });

        builder.Entity<AuthFlowState>(entity =>
        {
            entity.ToTable("AuthFlowStates");
            entity.Property(x => x.TokenHash).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Purpose).HasMaxLength(64).IsRequired();
            entity.Property(x => x.Payload).HasColumnType("jsonb");
            entity.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ExpiresAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.ConsumedAt).HasColumnType("timestamp with time zone");
            entity.HasIndex(x => x.TokenHash).IsUnique();
        });

        builder.Entity<AuditEvent>(entity =>
        {
            entity.ToTable("AuditEvents");
            entity.Property(x => x.EventType).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Metadata).HasColumnType("jsonb");
            entity.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
            entity.HasIndex(x => x.CreatedAt);
        });
    }

    private static readonly JsonSerializerOptions PasskeyJsonOptions = new(JsonSerializerDefaults.Web);

    private static readonly ValueConverter<IdentityPasskeyData, string> PasskeyDataConverter = new(
        value => JsonSerializer.Serialize(value, PasskeyJsonOptions),
        value => JsonSerializer.Deserialize<IdentityPasskeyData>(value, PasskeyJsonOptions)!);

    private static readonly ValueComparer<IdentityPasskeyData> PasskeyDataComparer = new(
        (left, right) => JsonSerializer.Serialize(left, PasskeyJsonOptions) == JsonSerializer.Serialize(right, PasskeyJsonOptions),
        value => JsonSerializer.Serialize(value, PasskeyJsonOptions).GetHashCode(),
        value => JsonSerializer.Deserialize<IdentityPasskeyData>(
                     JsonSerializer.Serialize(value, PasskeyJsonOptions),
                     PasskeyJsonOptions)!);
}
