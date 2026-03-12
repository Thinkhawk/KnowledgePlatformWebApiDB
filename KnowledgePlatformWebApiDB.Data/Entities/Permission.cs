namespace KnowledgePlatformWebApiDB.Data.Entities;

public class Permission
{
    public int Id { get; set; }

    public string Name { get; set; }

    // Navigation Property

    public ICollection<RolePermission> RolePermissions { get; set; }
}