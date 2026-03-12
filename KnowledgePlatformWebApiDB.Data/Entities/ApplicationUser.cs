namespace KnowledgePlatformWebApiDB.Data.Entities;

using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser<int>
{
    public string? FullName { get; set; }
    public bool IsActive { get; set; }

    // Navigation

    public ICollection<AuditEntry> AuditEntries { get; set; }
}

//ApplicationUser
//   │
//   ├── TeamAccess(Many)
//   └── AuditEntries(Many)