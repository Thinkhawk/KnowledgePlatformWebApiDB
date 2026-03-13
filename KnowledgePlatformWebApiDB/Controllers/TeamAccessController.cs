using KnowledgePlatformWebApiDB.DtoModels.TeamAccessDtos;
using KnowledgePlatformWebApiDB.DtoModels.TeamAccesses;
using KnowledgePlatformWebApiDB.Services.TeamAccesses;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgePlatformWebApiDB.Controllers;

[Route("api/[controller]")]
public sealed class TeamAccessController : BaseApiController
{
    private readonly TeamAccessService _teamAccessService;

    public TeamAccessController(TeamAccessService teamAccessService)
    {
        _teamAccessService = teamAccessService;
    }

    // CREATE Team Access
    [HttpPost]
    public async Task<IActionResult> CreateTeamAccess([FromBody] TeamAccessCreateDto createDto)
    {
        var result = await _teamAccessService.CreateAsync(createDto);
        return HandleResult(result);
    }

    // GET ALL Team Access records
    [HttpGet]
    public async Task<IActionResult> ReadAllTeamAccess()
    {
        var result = await _teamAccessService.ReadAllAsync();
        return HandleResult(result);
    }

    // GET ONE Team Access
    [HttpGet("{accessId:int}")]
    public async Task<IActionResult> ReadOneTeamAccess(int accessId)
    {
        var result = await _teamAccessService.ReadOneAsync(accessId);
        return HandleResult(result);
    }

    // UPDATE Team Access
    [HttpPut("{accessId:int}")]
    public async Task<IActionResult> UpdateTeamAccess(int accessId, [FromBody] TeamAccessUpdateDto updateDto)
    {
        var result = await _teamAccessService.UpdateAsync(accessId, updateDto);
        return HandleResult(result);
    }

    // DELETE Team Access
    [HttpDelete("{accessId:int}")]
    public async Task<IActionResult> DeleteTeamAccess(int accessId, [FromBody] TeamAccessDeleteDto deleteDto)
    {
        var result = await _teamAccessService.DeleteAsync(accessId, deleteDto);
        return HandleResult(result);
    }
}