using KnowledgePlatformWebApiDB.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace KnowledgePlatformWebApiDB.Data.Data;

public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser,ApplicationRole,string>
{

    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<Team> Teams { get; set; }
    public DbSet<TeamAccess> TeamAccesses { get; set; }
    public DbSet<Note> Notes { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        // --- RolePermission Constraints

        modelBuilder.Entity<RolePermission>(entity => {

            // 1. Configure Composite Key for the Bridge Table
            entity.HasKey(rp => new { rp.RoleId, rp.PermissionId });

            // 2. Define Many-to-Many: Role <-> Permission
            entity.HasOne(rp => rp.Role)
                .WithMany(r => r.RolePermissions)
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.Cascade); // If role is deleted, mapping is removed

            entity.HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.Cascade);
        });
            

        // --- Project Constraints

        modelBuilder.Entity<Project>(entity =>
        {

            entity.ToTable(t => t.HasCheckConstraint(
                name: "CK_Projects_Name_NotBlank",
                sql: "LEN(LTRIM(RTRIM(Name))) > 0"));

            entity.ToTable(t => t.HasCheckConstraint(
                name: "CK_Projects_Name_NotBlank",
                sql: "LEN(LTRIM(RTRIM(Name))) > 0"));

            entity.HasOne(p => p.User)
            .WithMany(u => u.Projects)
            .HasForeignKey(p => p.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(p => p.Name)
            .IsUnique()
            .HasDatabaseName("UQ_Projects_Name");

            entity.Property(p => p.RowVersion)
            .IsRowVersion();
        });


        // --- Team Constraints

        modelBuilder.Entity<Team>(entity => {

            entity.HasOne(tm => tm.Project)
            .WithMany(p => p.Teams)
            .HasForeignKey(tm => tm.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(tm => tm.User)
            .WithMany(u => u.Teams)
            .HasForeignKey(tm => tm.CreatorId)
            .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(tm => new { tm.Name, tm.ProjectId})
            .IsUnique()
            .HasDatabaseName("UQ_ProjectTeams_Name");

            entity.Property(tm => tm.RowVersion)
            .IsRowVersion();
        });


        // --- Team Access

        modelBuilder.Entity<TeamAccess>(entity => {

            entity.Property(ta => ta.AccessLevel)
            .HasConversion<string>();

            entity.ToTable(t => t.HasCheckConstraint(
                name: "CK_TeamAccesses_AccessLevel",
                sql: "[AccessLevel] IN ('Read', 'Write')"));

            entity.HasOne(ta => ta.Team)
            .WithMany(t => t.TeamAccesses)
            .HasForeignKey(ta => ta.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

        });

        // --- Note Constraints

        modelBuilder.Entity<Note>(entity => {

            entity.ToTable(t => t.HasCheckConstraint(
                name: "CK_Notes_Title_NotBlank",
                sql: "LEN(LTRIM(RTRIM(title))) > 0"));

            entity.ToTable(t => t.HasCheckConstraint(
                name: "CK_Notes_Content_NotBlank",
                sql: "LEN(LTRIM(RTRIM(content))) > 0"));

            var valueComparer = new ValueComparer<List<string>>(
                (c1, c2) => c1!.SequenceEqual(c2!),
                c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                c => c.ToList());

            entity.Property(n => n.Tags)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
            .Metadata.SetValueComparer(valueComparer);

            entity.HasIndex(n => new { n.Title, n.TeamId })
            .IsUnique()
            .HasDatabaseName("UQ_TeamNotes_Title");

            entity.HasOne(n => n.Team)
            .WithMany(t => t.Note)
            .HasForeignKey(n => n.TeamId)
            .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(n => n.User)
            .WithMany(u => u.Notes)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Restrict);

            entity.Property(n => n.RowVersion)
            .IsRowVersion();
        });
    }

    public override int SaveChanges()
    {
        ApplyAuditInformation();
        return base.SaveChanges();
    }


    public override Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditInformation();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        ApplyAuditInformation();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyAuditInformation()
    {
        var utcNow = DateTime.UtcNow;
        var entries = ChangeTracker.Entries<AuditableEntity>();

        foreach (var entry in entries)
        {
            if (entry.State == EntityState.Added)
            {
                entry.Entity.CreatedAtUtc = utcNow;
                entry.Entity.UpdatedAtUtc = utcNow;
            }

            if (entry.State == EntityState.Modified)
            {
                entry.Entity.UpdatedAtUtc = utcNow;
            }
        }
    }
}