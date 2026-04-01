namespace KnowledgePlatformWebApiDB.Auth.DTO
{
    public class ChangeRoleDto
    {
        public string Username { get; set; } = string.Empty;
        public string OldRole { get; set; } = string.Empty;
        public string NewRole { get; set; } = string.Empty;
    }
}
