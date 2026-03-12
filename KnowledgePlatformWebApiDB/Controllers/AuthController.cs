using KnowledgePlatformWebApiDB.Auth.DTO;
using KnowledgePlatformWebApiDB.Auth.Services;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgePlatformWebApiDB.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
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



        
    }
}