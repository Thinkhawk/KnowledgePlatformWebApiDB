namespace KnowledgePlatformWebApiDB.Data.Entities;

using Microsoft.AspNetCore.Identity;

public class ApplicationRole : IdentityRole<int>
{
    // Navigation

    public ICollection<RolePermission> RolePermissions { get; set; }
}
//ApplicationRole   Admin ,TeamLead ,User
//   │
//   ├── RolePermissions(Many)
//   └── TeamAccess(Many)
// already have the property like 

// Id
//Name    
//NormalizedName 
//ConcurrencyStamp    