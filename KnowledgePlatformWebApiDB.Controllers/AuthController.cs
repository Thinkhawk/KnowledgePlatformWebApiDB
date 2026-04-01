using KnowledgePlatformWebApiDB.Auth.DTO;
using KnowledgePlatformWebApiDB.Auth.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Security.Claims;

namespace KnowledgePlatformWebApiDB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : BaseApiController
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        // Helper: check if current caller is the seeded admin created by IdentitySeeder
        private bool IsSeededAdmin()
        {
            var currentUsername = User.FindFirstValue(ClaimTypes.Name) ?? User.Identity?.Name ?? string.Empty;
            var currentEmail = User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;

            return string.Equals(currentUsername, "admin", StringComparison.OrdinalIgnoreCase)
                   || string.Equals(currentEmail, "admin@test.com", StringComparison.OrdinalIgnoreCase);
        }

        // LOGIN API
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto loginDto)
        {
            var result = await _authService.LoginAsync(loginDto);

            if (!result.Success)
            {
                return Unauthorized(result.Message);
            }

            return Ok(new
            {
                message = result.Message,
                token = result.Token
            });
        }

        //Create User 
        [Authorize(Roles = "ProjectAdmin")]
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto dto)
        {
            var result = await _authService.CreateUserAsync(dto);

            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        // Change a user's role ( Only the seeded admin may change roles)
        [Authorize(Roles = "ProjectAdmin")]
        [HttpPost("change-role")]
        public async Task<IActionResult> ChangeUserRole([FromBody] ChangeRoleDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid request.");

            // Only the seeded admin may change roles
            if (!IsSeededAdmin())
                return Forbid();

            var result = await _authService.ChangeUserRoleAsync(dto.Username, dto.OldRole, dto.NewRole);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        // Delete a user (only the seeder admin) 
        [Authorize(Roles = "ProjectAdmin")]
        [HttpDelete("delete-user/{username}")]
        public async Task<IActionResult> DeleteUser([FromRoute] string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                return BadRequest("Username is required.");

            // Only the seeded admin may delete users
            if (!IsSeededAdmin())
                return Forbid();

            // Prevent seeded admin from deleting themselves (service also protects this)
            if (string.Equals(username, "admin", StringComparison.OrdinalIgnoreCase))
                return BadRequest("Cannot delete the seeded admin user.");

            var result = await _authService.DeleteUserAsync(username);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }


        // CHANGE PASSWORD 
        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            var email = User.FindFirstValue(ClaimTypes.Email);
            if (string.IsNullOrEmpty(email))
                return Unauthorized("Invalid token");

            var result = await _authService.ChangePasswordAsync(email, dto);
            if (!result.Success)
                return BadRequest(result.Message);

            return Ok(result.Message);
        }

        // Get all project leads (only the seeded admin can access).....
        [Authorize(Roles = "ProjectAdmin")]
        [HttpGet("project-leads")]
        public async Task<IActionResult> GetProjectLeads()
        {
            if (!IsSeededAdmin())
                return Forbid();

            var users = await _authService.GetProjectLeadsAsync();
            return Ok(users);
        }

        // Get all team members (only the seeded admin can access)...
        [Authorize(Roles = "ProjectAdmin")]
        [HttpGet("team-members")]
        public async Task<IActionResult> GetTeamMembers()
        {
            if (!IsSeededAdmin())
                return Forbid();

            var users = await _authService.GetTeamMembersAsync();
            return Ok(users);
        }

        // Get all project admins (only the seeded admin can access)
        [Authorize(Roles = "ProjectAdmin")]
        [HttpGet("project-admins")]
        public async Task<IActionResult> GetProjectAdmins()
        {
            if (!IsSeededAdmin())
                return Forbid();

            var users = await _authService.GetProjectAdminsAsync();

            var filtered = users
                .Where(u => !string.Equals(u.Username, "admin", StringComparison.OrdinalIgnoreCase)
                            && !string.Equals(u.Email, "admin@test.com", StringComparison.OrdinalIgnoreCase))
                .ToList();

            return Ok(filtered);
        }

    }
}
