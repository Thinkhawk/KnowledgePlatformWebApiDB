using KnowledgePlatformWebApiDB.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KnowledgePlatformWebApiDB.Data.Data;

public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser,ApplicationRole,string>
{

    //public DbSet<Team> Teams { get; set; }
    public DbSet<Note> Notes { get; set; }
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<AuditEntry> AuditEntries { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : base(options)
    {
    }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Configure Composite Key for the Bridge Table
        modelBuilder.Entity<RolePermission>()
            .HasKey(rp => new { rp.RoleId, rp.PermissionId });

        // 2. Define Many-to-Many: Role <-> Permission
        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade); // If role is deleted, mapping is removed

        modelBuilder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // 3. Define One-to-Many: User -> AuditEntries
        modelBuilder.Entity<AuditEntry>()
            .HasOne(a => a.User)
            .WithMany(u => u.AuditEntries)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Keep audit logs even if user is deleted (standard practice)

        modelBuilder.Entity<Note>()
            .ToTable(t => t.HasCheckConstraint(
                name: "CK_Notes_Title_NotBlank",
                sql: "LEN(LTRIM(RTRIM(title))) > 0"));

        modelBuilder.Entity<Note>()
            .ToTable(t => t.HasCheckConstraint(
                name: "CK_Notes_Content_NotBlank",
                sql: "LEN(LTRIM(RTRIM(content))) > 0"));

        modelBuilder.Entity<Note>()
            .Property(n => n.Tags)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            );
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