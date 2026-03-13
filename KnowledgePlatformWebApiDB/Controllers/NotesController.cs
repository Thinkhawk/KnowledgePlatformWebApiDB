using KnowledgePlatformWebApiDB.Data.Entities;
using KnowledgePlatformWebApiDB.DtoModels.Notes;
using KnowledgePlatformWebApiDB.Services.Notes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgePlatformWebApiDB.Controllers;

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
    [HttpGet]
    public async Task<IActionResult> ReadAllNotes()
    {
        var result = await _noteService.ReadAllAsync();
        return HandleResult(result);
    }


    [Authorize(Roles = "ProjectAdmin,ProjectLead,TeamMember")]
    [HttpGet("filter")]
    public async Task<IActionResult> ReadNotesWithFilter([FromQuery] NoteFilterDto filterDto)
    {
        var result = await _noteService.ReadWithFilterAsync(filterDto);
        return HandleResult(result);
    }


    [Authorize(Roles = "ProjectAdmin,ProjectLead,TeamMember")]
    [HttpPut("{noteId:guid}")]
    public async Task<IActionResult> UpdateNote(Guid noteId, NoteUpdateDto updateDto)
    {
        var result = await _noteService.UpdateAsync(noteId, updateDto);
        return HandleResult(result);
    }


    [Authorize(Roles = "ProjectAdmin,ProjectLead,TeamMember")]
    [HttpDelete]
    public async Task<IActionResult> DeleteNote(Guid noteId, NoteDeleteDto deleteDto)
    {
        var result = await _noteService.DeleteAsync(noteId, deleteDto);
        return HandleResult(result);
    }

}
