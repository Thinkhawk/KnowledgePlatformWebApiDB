
using KnowledgePlatformWebApiDB.Auth.DTO;
using KnowledgePlatformWebApiDB.Data.Entities;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace KnowledgePlatformWebApiDB.Auth.Services
{
    public class AuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly TokenService _tokenService;

        public AuthService(
        UserManager<ApplicationUser> userManager,
        TokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        // LOGIN METHOD
        public async Task<(bool Success, string Message, string? Token)> LoginAsync(LoginDto loginDto)
        {
            var user = await _userManager.FindByNameAsync(loginDto.Username);

            if (user == null)
            {
                return (false, "Invalid username or password.", null);
            }

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);

            if (!isPasswordValid)
            {
                return (false, "Invalid username or password.", null);
            }

            var roles = await _userManager.GetRolesAsync(user);

            var token = _tokenService.GenerateJwtToken(user, roles);

            return (true, "Login successful.", token);
        }


        //Create User Method
        public async Task<(bool Success, string Message)> CreateUserAsync(CreateUserDto dto)
        {
            var userExists = await _userManager.FindByNameAsync(dto.Username);

            if (userExists != null)
                return (false, "User already exists");

            var user = new ApplicationUser
            {
                UserName = dto.Username,
                Email = dto.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);

            if (!result.Succeeded)
            {
                string msg = "";
                foreach (var item in result.Errors)
                {
                    msg += item.Description + " ";
                }

                return (false, msg);
            }


            await _userManager.AddToRoleAsync(user, dto.Role);

            return (true, "User created successfully");
        }




        // Change Password Method
        public async Task<(bool Success, string Message)> ChangePasswordAsync(string email, ChangePasswordDto dto)
        {
            if (dto == null)
                return (false, "Invalid request.");

            if (string.IsNullOrWhiteSpace(email))
                return (false, "User email is required.");

            // Find user by email
            var user = await _userManager.FindByEmailAsync(email);

            if (user == null)
                return (false, "User not found");

            // Validate input
            if (string.IsNullOrWhiteSpace(dto.CurrentPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
                return (false, "Both current and new passwords are required.");

            if (dto.CurrentPassword == dto.NewPassword)
                return (false, "New password must be different from the current password.");

            // Change the password
            var result = await _userManager.ChangePasswordAsync(user, dto.CurrentPassword, dto.NewPassword);

            if (!result.Succeeded)
            {
                // Combine all errors into a single message
                var msg = string.Join(" ", result.Errors.Select(e => e.Description));
                return (false, msg);
            }

            return (true, "Password changed successfully");
        }

        // Get users by role
        public async Task<List<UserReadDto>> GetUsersByRoleAsync(string role)
        {
            var users = await _userManager.GetUsersInRoleAsync(role);
            var result = new List<UserReadDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);

                result.Add(new UserReadDto
                {
                    Id = user.Id,
                    Username = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    FullName = (user as ApplicationUser)?.FullName ?? string.Empty,
                    Roles = roles.ToList()
                });
            }

            return result;
        }

        public Task<List<UserReadDto>> GetProjectLeadsAsync() => GetUsersByRoleAsync("ProjectLead");
        public Task<List<UserReadDto>> GetTeamMembersAsync() => GetUsersByRoleAsync("TeamMember");
        public Task<List<UserReadDto>> GetProjectAdminsAsync() => GetUsersByRoleAsync("ProjectAdmin");








        public async Task<(bool Success, string Message)> ChangeUserRoleAsync(string username, string oldRole, string newRole)
        {
            if (string.IsNullOrWhiteSpace(username))
                return (false, "Username is required.");

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return (false, "User not found.");

            var roles = await _userManager.GetRolesAsync(user);

            // Allow changing any role (including ProjectAdmin). Caller must be ProjectAdmin (enforced at controller).
            if (!roles.Contains(oldRole))
                return (false, $"User does not have the role '{oldRole}'.");

            // Remove old role and add new role
            var removeResult = await _userManager.RemoveFromRoleAsync(user, oldRole);
            if (!removeResult.Succeeded)
            {
                var msg = string.Join(' ', removeResult.Errors.Select(e => e.Description));
                return (false, msg);
            }

            var addResult = await _userManager.AddToRoleAsync(user, newRole);
            if (!addResult.Succeeded)
            {
                // Try to restore the old role on failure
                await _userManager.AddToRoleAsync(user, oldRole);
                var msg = string.Join(' ', addResult.Errors.Select(e => e.Description));
                return (false, msg);
            }

            return (true, "Role changed successfully.");
        }

        // Delete a user
        public async Task<(bool Success, string Message)> DeleteUserAsync(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return (false, "Username is required.");

            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                return (false, "User not found.");

            var roles = await _userManager.GetRolesAsync(user);

            // Prevent deleting the seeded admin account (username 'admin' or email 'admin@test.com')
            if (string.Equals(user.UserName, "admin", System.StringComparison.OrdinalIgnoreCase)
            || string.Equals(user.Email, "admin@test.com", System.StringComparison.OrdinalIgnoreCase))
            {
                return (false, "Cannot delete the seeded admin user.");
            }

            // Only allow deletion of users that have one of the application roles
            var allowedRoles = new[] { "ProjectAdmin", "ProjectLead", "TeamMember" };
            if (!roles.Any(r => allowedRoles.Contains(r)))
            {
                return (false, "Only users with role ProjectAdmin, ProjectLead or TeamMember can be deleted.");
            }

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var msg = string.Join(' ', result.Errors.Select(e => e.Description));
                return (false, msg);
            }

            return (true, "User deleted successfully.");
        }




    }
}