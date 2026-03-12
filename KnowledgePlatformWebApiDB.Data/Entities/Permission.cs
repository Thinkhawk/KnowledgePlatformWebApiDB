using DemoWebApiDB.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace KnowledgePlatformWebApiDB.Data.Entities;

public class Permission
{
    public int Id { get; set; }

    public string Name { get; set; }

    // Navigation Property

    public ICollection<RolePermission> RolePermissions { get; set; }
}