using Microsoft.EntityFrameworkCore;

namespace Takt.People.API.Persistence;

public sealed class PeopleAppDbContext(DbContextOptions<PeopleAppDbContext> options) : DbContext(options)
{
    public DbSet<Person> People => Set<Person>();
    public DbSet<PersonalDataConsent> PersonalDataConsents => Set<PersonalDataConsent>();
    public DbSet<Branch> Branches => Set<Branch>();
    public DbSet<ClubMembership> ClubMemberships => Set<ClubMembership>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Person>(entity =>
        {
            entity.ToTable("People");
            entity.Property(x => x.FirstName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.LastName).HasMaxLength(128).IsRequired();
            entity.Property(x => x.FathersName).HasMaxLength(128);
            entity.Property(x => x.Alias).HasMaxLength(128).IsRequired();
            entity.Property(x => x.AnonymizationKeyHash).HasMaxLength(128);
            entity.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.AnonymizedAt).HasColumnType("timestamp with time zone");
            entity.HasIndex(x => x.Alias).IsUnique();
            entity.HasIndex(x => x.AnonymizationKeyHash);
            entity.HasMany(x => x.Memberships)
                .WithOne(x => x.Person)
                .HasForeignKey(x => x.PersonId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PersonalDataConsent>(entity =>
        {
            entity.ToTable("PersonalDataConsents");
            entity.Property(x => x.Version).HasMaxLength(32).IsRequired();
            entity.Property(x => x.AcceptedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.AcceptedFromIp).HasMaxLength(64);
            entity.HasOne(x => x.Person)
                .WithOne(x => x.PersonalDataConsent)
                .HasForeignKey<PersonalDataConsent>(x => x.PersonId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => x.PersonId).IsUnique();
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.ToTable("Branches");
            entity.Property(x => x.Name).HasMaxLength(128).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(64);
            entity.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
            entity.HasIndex(x => x.Name).IsUnique();
            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<ClubMembership>(entity =>
        {
            entity.ToTable("ClubMemberships");
            entity.Property(x => x.RoleId).HasConversion<int>();
            entity.Property(x => x.StartDate).HasColumnType("date");
            entity.Property(x => x.FinishDate).HasColumnType("date");
            entity.Property(x => x.CreatedAt).HasColumnType("timestamp with time zone");
            entity.Property(x => x.UpdatedAt).HasColumnType("timestamp with time zone");
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_ClubMemberships_RoleId", "\"RoleId\" IN (101, 102, 201, 202, 301)");
                table.HasCheckConstraint("CK_ClubMemberships_FinishDate", "\"FinishDate\" IS NULL OR \"FinishDate\" >= \"StartDate\"");
            });
            entity.HasOne(x => x.Branch)
                .WithMany(x => x.Memberships)
                .HasForeignKey(x => x.BranchId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.PersonId);
            entity.HasIndex(x => x.BranchId);
            entity.HasIndex(x => x.RoleId);
            entity.HasIndex(x => new { x.PersonId, x.RoleId, x.BranchId, x.StartDate });
        });
    }
}
