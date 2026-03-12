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

        // CREATE USER METHOD
        public async Task<(bool Success, string Message)> CreateUserAsync(CreateUserDto dto)
        {
            var existingUser = await _userManager.FindByNameAsync(dto.Username);

            if (existingUser != null)
            {
                return (false, "User already exists.");
            }

            var user = new ApplicationUser
            {
                UserName = dto.Username,
                Email = dto.Email,
                FullName = dto.FullName,
                EmailConfirmed = false
            };

            //default password for the new added user 
            string defaultPassword = "Temp@123456";

            var result = await _userManager.CreateAsync(user, defaultPassword);

            if (!result.Succeeded)
            {
                return (false, "User creation failed.");
            }

            await _userManager.AddToRoleAsync(user, dto.Role);

            return (true, "User created successfully with default password.");
        }


        public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordDto dto)
        {
            var user = await _userManager.FindByEmailAsync(dto.Email);

            if (user == null)
                return (false, "User not found.");

            var result = await _userManager.ResetPasswordAsync(user, dto.Token, dto.NewPassword);

            if (!result.Succeeded)
                return (false, "Password reset failed.");

            return (true, "Password reset successful.");
        }
    }
}