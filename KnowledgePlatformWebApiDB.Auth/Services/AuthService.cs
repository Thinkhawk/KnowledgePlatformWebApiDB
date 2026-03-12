using KnowledgePlatformWebApiDB.Auth.DTO;
using KnowledgePlatformWebApiDB.Data.Entities;
using Microsoft.AspNetCore.Identity;

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



    }
}