using KnowledgePlatformWebApiDB.DtoModels.Projects;
using KnowledgePlatformWebApiDB.Services.Projects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgePlatformWebApiDB.Controllers;

// All endpoints require a valid JWT token.
// ProjectAdmin: full access — create, read, update, delete.
// ProjectLead, TeamMember: read only.
[Authorize]
[Route("api/[controller]")]
public sealed class ProjectsController : BaseApiController
{
    private readonly ProjectService _projectService;

    public ProjectsController(ProjectService projectService)
    {
        _projectService = projectService;
    }

    // CREATE PROJECT
    [Authorize(Roles = "ProjectAdmin")]
    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] ProjectCreateDto createDto)
    {
        var result = await _projectService.CreateAsync(createDto);
        return HandleResult(result);
    }

    // GET ALL PROJECTS
    [Authorize(Roles = "ProjectAdmin")]
    [HttpGet]
    public async Task<IActionResult> ReadAllProjects()
    {
        var result = await _projectService.ReadAllAsync();
        return HandleResult(result);
    }

    // GET ONE PROJECT
    [Authorize(Roles = "ProjectAdmin")]
    [HttpGet("{projectId:int}")]
    public async Task<IActionResult> ReadOneProject(int projectId)
    {
        var result = await _projectService.ReadOneAsync(projectId);
        return HandleResult(result);
    }

    // UPDATE PROJECT
    [Authorize(Roles = "ProjectAdmin")]
    [HttpPut("{projectId:int}")]
    public async Task<IActionResult> UpdateProject(int projectId, [FromBody] ProjectUpdateDto updateDto)
    {
        var result = await _projectService.UpdateAsync(projectId, updateDto);
        return HandleResult(result);
    }

    // DELETE PROJECT
    [Authorize(Roles = "ProjectAdmin")]
    [HttpDelete("{projectId:int}")]
    public async Task<IActionResult> DeleteProject(int projectId, [FromBody] ProjectDeleteDto deleteDto)
    {
        var result = await _projectService.DeleteAsync(projectId, deleteDto);
        return HandleResult(result);
    }
}