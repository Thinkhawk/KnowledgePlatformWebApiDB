public class RolePermission
{
    public string RoleId { get; set; }      // string RoleId
    public ApplicationRole Role { get; set; }

    public int PermissionId { get; set; }   // integer PermissionId
    public Permission Permission { get; set; }
}