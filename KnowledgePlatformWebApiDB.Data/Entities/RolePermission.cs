
using KnowledgePlatformWebApiDB.Data.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace DemoWebApiDB.Data.Entities;

public class RolePermission
{
    public int RoleId { get; set; }

    public int PermissionId { get; set; }

    // Navigation

    public ApplicationRole Role { get; set; }

    public Permission Permission { get; set; }
}