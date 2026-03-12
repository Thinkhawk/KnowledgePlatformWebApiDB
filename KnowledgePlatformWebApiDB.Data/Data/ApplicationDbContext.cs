using DemoWebApiDB.Data.Entities;
using KnowledgePlatformWebApiDB.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KnowledgePlatformWebApiDB.Data.Data;

public class ApplicationDbContext
    : IdentityDbContext<ApplicationUser, ApplicationRole, int>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Register custom security tables
    public DbSet<Permission> Permissions { get; set; }
    public DbSet<RolePermission> RolePermissions { get; set; }
    public DbSet<AuditEntry> AuditEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // CRITICAL: This configures the default Identity tables (AspNetUsers, etc.)
        base.OnModelCreating(builder);

        // 1. Configure Composite Key for the Bridge Table
        builder.Entity<RolePermission>()
            .HasKey(rp => new { rp.RoleId, rp.PermissionId });

        // 2. Define Many-to-Many: Role <-> Permission
        builder.Entity<RolePermission>()
            .HasOne(rp => rp.Role)
            .WithMany(r => r.RolePermissions)
            .HasForeignKey(rp => rp.RoleId)
            .OnDelete(DeleteBehavior.Cascade); // If role is deleted, mapping is removed

        builder.Entity<RolePermission>()
            .HasOne(rp => rp.Permission)
            .WithMany(p => p.RolePermissions)
            .HasForeignKey(rp => rp.PermissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // 3. Define One-to-Many: User -> AuditEntries
        builder.Entity<AuditEntry>()
            .HasOne(a => a.User)
            .WithMany(u => u.AuditEntries)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Restrict); // Keep audit logs even if user is deleted (standard practice)
    }
}