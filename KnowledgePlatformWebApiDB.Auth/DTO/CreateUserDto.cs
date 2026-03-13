using System.ComponentModel.DataAnnotations;

namespace KnowledgePlatformWebApiDB.Auth.DTO
{
    public class CreateUserDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;

        [Required]
        public string Role { get; set; } = string.Empty;
    }
}

//Dto for the createUser..