using System;
using System.Collections.Generic;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace KnowledgePlatformWebApiDB.Data.Entities;

using Microsoft.AspNetCore.Identity;

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