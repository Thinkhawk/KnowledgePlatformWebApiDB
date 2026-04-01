using KnowledgePlatformWebApiDB.Data.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace KnowledgePlatformWebApiDB.Controllers;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public sealed class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    // GET /api/users?search={term}
    // Returns up to 10 users whose username or email contains the search term.
    [HttpGet]
    public async Task<IActionResult> SearchUsers([FromQuery] string? search)
    {
        if (string.IsNullOrWhiteSpace(search) || search.Trim().Length < 2)
            return Ok(Array.Empty<object>());

        var term = search.Trim().ToLower();

        var users = await _userManager.Users
            .Where(u => u.UserName!.ToLower().Contains(term) ||
                        u.Email!.ToLower().Contains(term))
            .OrderBy(u => u.UserName)
            .Take(10)
            .Select(u => new
            {
                userId = u.Id,
                fullName = u.FullName != null && u.FullName != "" ? u.FullName : u.UserName,
                email = u.Email
            })
            .ToListAsync();

        return Ok(users);
    }

    // GET /api/users/search?email={email}
    [HttpGet("search")]
    public async Task<IActionResult> SearchByEmail([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return BadRequest(new { detail = "Email is required." });

        var user = await _userManager.FindByEmailAsync(email.Trim());

        if (user is null)
            return NotFound(new { detail = "No user found with that email." });

        return Ok(new
        {
            userId = user.Id,
            fullName = string.IsNullOrWhiteSpace(user.FullName) ? user.UserName : user.FullName,
            email = user.Email
        });
    }
}
