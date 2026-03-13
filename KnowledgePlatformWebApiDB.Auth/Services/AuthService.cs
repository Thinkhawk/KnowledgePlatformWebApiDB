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
                foreach(var item in result.Errors)
                {
                    msg += item.Description+" ";
                }
                
                return (false, msg);
            }
                

            await _userManager.AddToRoleAsync(user, dto.Role);

            return (true, "User created successfully");
        }



    }
}