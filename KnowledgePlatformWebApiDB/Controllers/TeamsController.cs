using KnowledgePlatformWebApiDB.DtoModels.Teams;
using KnowledgePlatformWebApiDB.Services.Teams;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgePlatformWebApiDB.Controllers;

[Route("api/[controller]")]
public sealed class TeamsController : BaseApiController
{
    private readonly TeamService _teamService;

    public TeamsController(TeamService teamService)
    {
        _teamService = teamService;
    }

    // CREATE TEAM
    [Authorize(Roles = "ProjectAdmin,ProjectLead")]
    [HttpPost]
    public async Task<IActionResult> CreateTeam([FromBody] TeamCreateDto createDto)
    {
        var result = await _teamService.CreateAsync(createDto);
        return HandleResult(result);
    }

    // GET ALL TEAMS
    [Authorize(Roles = "ProjectAdmin,ProjectLead")]
    [HttpGet]
    public async Task<IActionResult> ReadAllTeams()
    {
        var result = await _teamService.ReadAllAsync();
        return HandleResult(result);
    }

    // GET ONE TEAM
    [Authorize(Roles = "ProjectAdmin,ProjectLead")]
    [HttpGet("{teamId:int}")]
    public async Task<IActionResult> ReadOneTeam(int teamId)
    {
        var result = await _teamService.ReadOneAsync(teamId);
        return HandleResult(result);
    }

    // UPDATE TEAM
    [Authorize(Roles = "ProjectAdmin,ProjectLead")]
    [HttpPut("{teamId:int}")]
    public async Task<IActionResult> UpdateTeam(int teamId, [FromBody] TeamUpdateDto updateDto)
    {
        var result = await _teamService.UpdateAsync(teamId, updateDto);
        return HandleResult(result);
    }

    // DELETE TEAM
    [Authorize(Roles = "ProjectAdmin,ProjectLead")]
    [HttpDelete("{teamId:int}")]
    public async Task<IActionResult> DeleteTeam(int teamId, [FromBody] TeamDeleteDto deleteDto)
    {
        var result = await _teamService.DeleteAsync(teamId, deleteDto);
        return HandleResult(result);
    }
}