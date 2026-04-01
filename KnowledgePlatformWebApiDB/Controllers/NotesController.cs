using KnowledgePlatformWebApiDB.Data.Entities;
using KnowledgePlatformWebApiDB.DtoModels.Notes;
using KnowledgePlatformWebApiDB.Services.Notes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgePlatformWebApiDB.Controllers;

[Authorize]
[Route("api/[controller]")]
public sealed class NotesController : BaseApiController
{

    private readonly NoteService _noteService;

    public NotesController(NoteService noteService)
    {
        _noteService = noteService;
    }


    [Authorize(Roles = "ProjectAdmin,ProjectLead,TeamMember")]
    [HttpPost]
    public async Task<IActionResult> CreateNote([FromBody] NoteCreateDto NotecreateDto)
    {
        var result = await _noteService.CreateAsync(NotecreateDto);
        return HandleResult(result);
    }

    [Authorize(Roles = "ProjectAdmin,ProjectLead,TeamMember")]
    [HttpGet("{noteId:guid}")]
    public async Task<IActionResult> ReadOneNote(Guid noteId)
    {
        var result = await _noteService.ReadOneAsync(noteId);
        return HandleResult(result);
    }

    [Authorize(Roles = "ProjectAdmin,ProjectLead,TeamMember")]
    [HttpGet]
    public async Task<IActionResult> ReadAllNotes()
    {
        var result = await _noteService.ReadAllAsync();
        return HandleResult(result);
    }

    [Authorize(Roles = "ProjectAdmin,ProjectLead,TeamMember")]
    [HttpGet("{teamId:int}")]
    public async Task<IActionResult> ReadNotesWithTeamId(int teamId)
    {
        var result = await _noteService.ReadWithTeamIdAsync(teamId);
        return HandleResult(result);
    }

    [Authorize(Roles = "ProjectAdmin,ProjectLead,TeamMember")]
    [HttpGet("{teamId:int}/filter")]
    public async Task<IActionResult> ReadNotesWithFilter(int teamId, [FromQuery] NoteFilterDto filterDto)
    {
        var result = await _noteService.ReadWithFilterAsync(teamId, filterDto);
        return HandleResult(result);
    }


    [Authorize(Roles = "ProjectAdmin,ProjectLead,TeamMember")]
    [HttpPut("{noteId:guid}")]
    public async Task<IActionResult> UpdateNote(Guid noteId, [FromBody] NoteUpdateDto updateDto)
    {
        var result = await _noteService.UpdateAsync(noteId, updateDto);
        return HandleResult(result);
    }


    [Authorize(Roles = "ProjectAdmin,ProjectLead,TeamMember")]
    [HttpDelete("{noteId:guid}")]
    public async Task<IActionResult> DeleteNote(Guid noteId, [FromBody] NoteDeleteDto deleteDto)
    {
        var result = await _noteService.DeleteAsync(noteId, deleteDto);
        return HandleResult(result);
    }

}
 