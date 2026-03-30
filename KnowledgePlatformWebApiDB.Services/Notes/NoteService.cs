using DemoWebApiDB.Infrastructure.Extensions;
using KnowledgePlatformWebApiDB.Data.Data;
using KnowledgePlatformWebApiDB.Data.Entities;
using KnowledgePlatformWebApiDB.DtoModels.Notes;
using KnowledgePlatformWebApiDB.Infrastructure.Helpers;
using KnowledgePlatformWebApiDB.Infrastructure.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text.Json;

namespace KnowledgePlatformWebApiDB.Services.Notes;

public sealed class NoteService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<NoteService> _logger;

    public NoteService(ApplicationDbContext dbContext, ILogger<NoteService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<string>> CreateAsync(NoteCreateDto createDto)
    {

        var title = createDto.Title.TrimOrEmpty();

        if (!title.HasValue())
        {
            _logger.LogWarning("Note Creation Failed: Title is empty.");

            return Result<string>.ValidationFailure(new[] {
                new ValidationErrorModel(nameof(createDto.Title), "Title cannot be empty or blank.")
            });
        }


        bool duplicateTitleExists = await _dbContext.Notes.AnyAsync(n => n.Title!.ToUpper() == createDto.Title.NormalizeKey());
        if (duplicateTitleExists)
        {
            _logger.LogWarning("Note Creation Failed: Duplicate Title '{Title}' attempted.", title);

            return Result<string>.ValidationFailure(new[] {
                new ValidationErrorModel(nameof(createDto.Title), $"Title '{title}' already exists.")
            });
        }

        var note = new Note
        {
            Title = title,
            Content = createDto.Content.NullIfWhiteSpace(),
            Tags = createDto.Tags,
            TeamId = createDto.TeamId,
            CreatorId = createDto.CreatorId,
            UpdaterId = createDto.CreatorId
        };

        _dbContext.Notes.Add(note);

        await _dbContext.SaveChangesAsync();

        var location = $"/api/notes/{note.NoteId}";
        _logger.LogInformation("Note created successfully. NoteId: {NoteId}, Title: {Title}", note.NoteId, note.Title);

        return Result<string>.Created(location);
    }

    public async Task<Result<NoteReadDto>> ReadOneAsync(Guid id)
    {
        var note = await _dbContext.Notes
            .AsNoTracking()
            .Include(n => n.Creator)
            .Include(n => n.Updater)
            .FirstOrDefaultAsync(n => n.NoteId.Equals(id));

        if (note is null)
        {
            _logger.LogWarning("Note retrieval failed: NoteId {NoteId} not found.", id);

            return Result<NoteReadDto>.NotFound($"Note with id '{id}' not found.");
        }

        var readDto = new NoteReadDto(
            NoteId: note.NoteId,
            Title: note.Title!,
            Content: note.Content!,
            Tags: note.Tags,
            TeamId: note.TeamId,
            CreatorId: note.CreatorId,
            CreatorName: note.Creator?.UserName ?? "Unknown User",
            CreatedAtUtc: note.CreatedAtUtc,
            UpdaterId: note.UpdaterId ?? "Never Updated",
            UpdaterName: note.Updater?.UserName ?? "Never Updated",
            UpdatedAtUtc: note.UpdatedAtUtc,
            RowVersion: RowVersionHelper.ToBase64(note.RowVersion)
        );

        _logger.LogInformation("Note retrieved successfully. NoteId: {NoteId}", id);

        return Result<NoteReadDto>.Success(readDto);
    }

    public async Task<Result<IReadOnlyList<NoteReadDto>>> ReadAllAsync()
    {
        var notes = await _dbContext.Notes
            .AsNoTracking()
            .Include(n => n.Creator)
            .Include(n => n.Updater)
            .OrderBy(n => n.UpdatedAtUtc)
            .ToListAsync();


        var readDtos = notes.Select(note => new NoteReadDto(
            NoteId: note.NoteId,
            Title: note.Title!,
            Content: note.Content!,
            Tags: note.Tags,
            TeamId: note.TeamId,
            CreatorId: note.CreatorId,
            CreatorName: note.Creator?.UserName ?? "Unknown User",
            CreatedAtUtc: note.CreatedAtUtc,
            UpdaterId: note.UpdaterId ?? "Never Updated",
            UpdaterName: note.Updater?.UserName ?? "Never Updated",
            UpdatedAtUtc: note.UpdatedAtUtc,
            RowVersion: RowVersionHelper.ToBase64(note.RowVersion)

            )).ToList().AsReadOnly();

        _logger.LogInformation("Retrived {NotesCount} notes.", readDtos.Count);

        return Result<IReadOnlyList<NoteReadDto>>.Success(readDtos);
    }

    public async Task<Result<IReadOnlyList<NoteReadDto>>> ReadWithTeamIdAsync(int teamId)
    {
        var notes = await _dbContext.Notes
            .AsNoTracking()
            .Include(n => n.Creator)
            .Include(n => n.Updater)
            .Where(n => n.TeamId == teamId)
            .OrderBy(n => n.UpdatedAtUtc)
            .ToListAsync();

        var readDtos = notes.FindAll(note => note.TeamId.Equals(teamId)).Select(note => new NoteReadDto(
            NoteId: note.NoteId,
            Title: note.Title!,
            Content: note.Content!,
            Tags: note.Tags,
            TeamId: note.TeamId,
            CreatorId: note.CreatorId,
            CreatorName: note.Creator?.UserName ?? "Unknown User",
            CreatedAtUtc: note.CreatedAtUtc,
            UpdaterId: note.UpdaterId ?? "Never Updated",
            UpdaterName: note.Updater?.UserName ?? "Never Updated",
            UpdatedAtUtc: note.UpdatedAtUtc,
            RowVersion: RowVersionHelper.ToBase64(note.RowVersion)

            )).ToList().AsReadOnly();

        _logger.LogInformation("Retrived {NotesCount} notes.", readDtos.Count);

        return Result<IReadOnlyList<NoteReadDto>>.Success(readDtos);
    }

    public async Task<Result<IReadOnlyList<NoteReadDto>>> ReadWithFilterAsync(int teamId, NoteFilterDto filterDto)
    {
        var query = _dbContext.Notes
            .Include(n => n.Creator)
            .Include(n => n.Updater)
            .AsNoTracking()
            .Where(n => n.TeamId == teamId);

        if (filterDto.NoteId.HasValue && filterDto.NoteId != Guid.Empty)
        {
            query = query.Where(n => n.NoteId == filterDto.NoteId);
        }

        if (!string.IsNullOrWhiteSpace(filterDto.Title))
        {
            var searchTitle = filterDto.Title.NormalizeKey();
                                                          
            query = query.Where(n => n.Title == searchTitle);
        }

        if (!string.IsNullOrWhiteSpace(filterDto.CreatorId))
        {
            var searchUser = filterDto.CreatorId.NormalizeKey();
            query = query.Where(n => n.CreatorId == searchUser);
        }

        if (!string.IsNullOrWhiteSpace(filterDto.UpdaterId))
        {
            var searchUser = filterDto.UpdaterId.NormalizeKey();
            query = query.Where(n => n.UpdaterId == searchUser);
        }

        if (filterDto.CreatedAtUtc.HasValue)
        {
            query = query.Where(n => n.CreatedAtUtc.Date == filterDto.CreatedAtUtc.Value.Date);
        }
        if (filterDto.UpdatedAtUtc.HasValue)
        {
            query = query.Where(n => n.UpdatedAtUtc!.Value.Date == filterDto.UpdatedAtUtc.Value.Date);
        }

        var notesList = await query.ToListAsync();

        if (filterDto.Tags?.Any() == true)
        {
            notesList = notesList.Where(note =>
                filterDto.Tags.All(filterTag =>
                    note.Tags != null && note.Tags.Contains(filterTag))
            ).ToList();
        }

        var notes = notesList.Select(note => new NoteReadDto(
            NoteId: note.NoteId,
            Title: note.Title!,
            Content: note.Content!,
            Tags: note.Tags,
            TeamId: note.TeamId,
            CreatorId: note.CreatorId,
            CreatorName: note.Creator?.UserName ?? "Unknown User",
            CreatedAtUtc: note.CreatedAtUtc,
            UpdaterId: note.UpdaterId ?? "Never Updated",
            UpdaterName: note.Updater?.UserName ?? "Never Updated",
            UpdatedAtUtc: note.UpdatedAtUtc,
            RowVersion: RowVersionHelper.ToBase64(note.RowVersion)
        )).ToList();

        if (notes.Count == 0)
        {
            _logger.LogWarning("No notes found for team {TeamId} with provided filters.", teamId);
        }

        _logger.LogInformation("Retrieved {NotesCount} notes.", notes.Count);
        return Result<IReadOnlyList<NoteReadDto>>.Success(notes.AsReadOnly());
    }

    public async Task<Result> UpdateAsync(Guid routeNoteId, NoteUpdateDto updateDto)
    {
        if (routeNoteId != updateDto.NoteId)
        {
            _logger.LogWarning("Note update failed: NoteId in route {RouteNoteId} doesn't match the NoteId in payload {PayloadNoteId}.", routeNoteId, updateDto.NoteId);
            return Result.ValidationFailure(new[] {
                new ValidationErrorModel(nameof(updateDto.NoteId), $"Route NoteId {routeNoteId} and payload NoteId {updateDto.NoteId} must match.")
            });
        }

        var note = await _dbContext.Notes.FirstOrDefaultAsync(n => n.NoteId == routeNoteId);

        if (note is null)
        {
            _logger.LogWarning("Note retrieval failed: NoteId: {NoteId} is not found.", routeNoteId);
            return Result.NotFound($"NoteId {routeNoteId} is not found.");
        }

        byte[] incomingRowVersion;
        try
        {
            incomingRowVersion = RowVersionHelper.FromBase64(updateDto.RowVersion);
        }
        catch (ArgumentException)
        {
            _logger.LogWarning("Note update failed due to invalid RowVersion format. NoteId: {NoteId}", routeNoteId);
            return Result.ValidationFailure(new[] {
                new ValidationErrorModel(nameof(updateDto.RowVersion),"Invalid RowVersion format.")
            });
        }

        _dbContext.Entry(note)
            .Property(n => n.RowVersion)
            .OriginalValue = incomingRowVersion;

        var title = updateDto.Title.NullIfWhiteSpace();
        if (title is null)
        {
            _logger.LogWarning("Note update failed: Empty title provided. NotedId: {NoteId}", routeNoteId);
            return Result.ValidationFailure(new[] {
                new ValidationErrorModel(nameof(updateDto.Title),"Title cannot be empty or blank.")
            });
        }

        bool duplicateTitleExists = await _dbContext.Notes.AnyAsync(n => n.Title!.ToUpper() == title.NormalizeKey() && n.NoteId != routeNoteId);
        if (duplicateTitleExists)
        {
            _logger.LogWarning("Note Creation Failed: Duplicate Title '{Title}' attempted.", title);

            return Result.ValidationFailure(new[] {
                new ValidationErrorModel(nameof(updateDto.Title), $"Title '{title}' already exists.")
            });
        }

        note.Title = title;
        note.Content = updateDto.Content.NullIfWhiteSpace();
        note.Tags = updateDto.Tags;
        note.UpdaterId = updateDto.UpdaterId!;

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            _logger.LogWarning("Note update concurrency conflict. NoteId: {NoteId}", routeNoteId);
            return Result.Concurrency($"The note with is {routeNoteId} was updated bu another user. Please reload and try again.");
        }

        _logger.LogInformation("Note updated successfully. NoteId: {NoteId}", routeNoteId);

        return Result.Accepted();
    }

    public async Task<Result> DeleteAsync(Guid routeNoteId, NoteDeleteDto deleteDto)
    {
        Console.WriteLine("Entered ---- "+routeNoteId);
        if (routeNoteId != deleteDto.NoteId)
        {
            _logger.LogWarning("Note delete failed: NoteId in route {RouteNoteId} doesn't match the NoteId in payload {PayloadNoteId}.", routeNoteId, deleteDto.NoteId);
            return Result.ValidationFailure(new[] {
                new ValidationErrorModel(nameof(deleteDto.NoteId), $"Route NoteId {routeNoteId} and payload NoteId {deleteDto.NoteId} must match.")
            });
        }

        var note = await _dbContext.Notes.FirstOrDefaultAsync(n => n.NoteId == deleteDto.NoteId);
        if (note is null)
        {
            _logger.LogWarning("Note delete failed: NoteId: {NoteId} is not found.", routeNoteId);
            return Result.NotFound($"NoteId {routeNoteId} is not found.");
        }

        byte[] incomingRowVersion;
        try
        {
            incomingRowVersion = RowVersionHelper.FromBase64(deleteDto.RowVersion);
        }
        catch (ArgumentException)
        {
            _logger.LogWarning("Note delete failed due to invalid RowVersion format. NoteId: {NoteId}", routeNoteId);
            return Result.ValidationFailure(new[] {
                new ValidationErrorModel(nameof(deleteDto.RowVersion),"Invalid RowVersion format.")
            });
        }

        _dbContext.Entry(note)
            .Property(n => n.RowVersion)
            .OriginalValue = incomingRowVersion;

        _dbContext.Notes.Remove(note);

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            _logger.LogWarning("Note delete concurrency conflict. NoteId: {NoteId}", routeNoteId);
            return Result.Concurrency($"The note with is {routeNoteId} was updated but another user. Please reload and try again.");
        }

        _logger.LogInformation("Note delete successfully. NoteId: {NoteId}", routeNoteId);

        return Result.Success();
    }
}
