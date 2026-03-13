using KnowledgePlatformWebApiDB.Services.UserAccess;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgePlatformWebApiDB.Controllers;

[Route("api/[controller]")]
public sealed class UserAccessController : BaseApiController
{
    private readonly UserAccessService _userAccessService;

    public UserAccessController(UserAccessService userAccessService)
    {
        _userAccessService = userAccessService;
    }

    /// <summary>
    /// GET: api/UserAccess/{userId}
    /// Retrieves all projects and teams a specific user has access to.
    /// </summary>
    /// <param name="userId">The ID of the user.</param>
    /// <returns>A hierarchical list of projects and their accessible teams.</returns>
    [HttpGet("{userId}")]
    public async Task<IActionResult> GetUserAccess(string userId)
    {
        // Calls the service logic created in the previous step
        var result = await _userAccessService.GetUserAccessibleProjectsAndTeamsAsync(userId);

        // HandleResult automatically maps Success to Ok(data), NotFound to 404, etc.
        return HandleResult(result);
    }
}