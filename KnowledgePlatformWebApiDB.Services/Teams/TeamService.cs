using DemoWebApiDB.Infrastructure.Extensions;
using KnowledgePlatformWebApiDB.Data.Data;
using KnowledgePlatformWebApiDB.Data.Entities;
using KnowledgePlatformWebApiDB.DtoModels.Teams;
using KnowledgePlatformWebApiDB.Infrastructure.Helpers;
using KnowledgePlatformWebApiDB.Infrastructure.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KnowledgePlatformWebApiDB.Services.Teams;

public sealed class TeamService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<TeamService> _logger;

    public TeamService(
        ApplicationDbContext db,
        ILogger<TeamService> logger)
    {
        _dbContext = db;
        _logger = logger;
    }


    /// <summary>
    /// Creates a new Team
    /// </summary>
    public async Task<Result<string>> CreateAsync(TeamCreateDto dto)
    {
        var cleanedName = dto.Name.TrimOrEmpty();

        if (!cleanedName.HasValue())
        {
            return Result<string>.ValidationFailure(new[]
            {
                new ValidationErrorModel(nameof(dto.Name),
                "Team name cannot be empty.")
            });
        }

        // Check if Project exists
        bool projectExists = await _dbContext.Projects
            .AnyAsync(p => p.ProjectId == dto.ProjectId);

        if (!projectExists)
        {
            return Result<string>.NotFound(
                $"Project with id '{dto.ProjectId}' not found.");
        }

        var entity = new Team
        {
            Name = cleanedName,
            ProjectId = dto.ProjectId
        };

        _dbContext.Teams.Add(entity);

        await _dbContext.SaveChangesAsync();

        var location = $"/api/teams/{entity.TeamId}";

        _logger.LogInformation(
            "Team created successfully. TeamId: {TeamId}",
            entity.TeamId);

        return Result<string>.Created(location);
    }


    /// <summary>
    /// Get one team by Id
    /// </summary>
    public async Task<Result<TeamReadDto>> ReadOneAsync(int id)
    {
        var entity = await _dbContext.Teams
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.TeamId == id);

        if (entity is null)
        {
            return Result<TeamReadDto>.NotFound(
                $"Team with id '{id}' not found.");
        }

        var dto = new TeamReadDto(
            TeamId: entity.TeamId,
            ProjectId: entity.ProjectId,
            Name: entity.Name,
            CreatedAtUtc: entity.CreatedAtUtc,
            ModifiedAtUtc: entity.ModifiedAtUtc,
            RowVersion: RowVersionHelper.ToBase64(entity.RowVersion)
        );

        return Result<TeamReadDto>.Success(dto);
    }


    /// <summary>
    /// Get all teams
    /// </summary>
    public async Task<Result<IReadOnlyList<TeamReadDto>>> ReadAllAsync()
    {
        var entities = await _dbContext.Teams
            .AsNoTracking()
            .OrderBy(t => t.Name)
            .ToListAsync();

        var dtos = entities
            .Select(e => new TeamReadDto(
                e.TeamId,
                e.ProjectId,
                e.Name,
                e.CreatedAtUtc,
                e.ModifiedAtUtc,
                RowVersionHelper.ToBase64(e.RowVersion)
            ))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<TeamReadDto>>.Success(dtos);
    }


    /// <summary>
    /// Update team
    /// </summary>
    public async Task<Result> UpdateAsync(int routeId, TeamUpdateDto dto)
    {
        if (routeId != dto.TeamId)
        {
            return Result.ValidationFailure(new[]
            {
                new ValidationErrorModel(
                    nameof(dto.TeamId),
                    "Route ID and payload ID must match.")
            });
        }

        var entity = await _dbContext.Teams
            .FirstOrDefaultAsync(t => t.TeamId == routeId);

        if (entity is null)
        {
            return Result.NotFound(
                $"Team with id '{routeId}' not found.");
        }

        byte[] incomingRowVersion;

        try
        {
            incomingRowVersion =
                RowVersionHelper.FromBase64(dto.RowVersion);
        }
        catch
        {
            return Result.ValidationFailure(new[]
            {
                new ValidationErrorModel(
                    nameof(dto.RowVersion),
                    "Invalid RowVersion format.")
            });
        }

        _dbContext.Entry(entity)
            .Property(e => e.RowVersion)
            .OriginalValue = incomingRowVersion;

        entity.Name = dto.Name.TrimOrEmpty();

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Concurrency(
                "The record was modified by another user.");
        }

        _logger.LogInformation(
            "Team updated successfully. TeamId: {TeamId}",
            routeId);

        return Result.Accepted();
    }


    /// <summary>
    /// Delete team
    /// </summary>
    public async Task<Result> DeleteAsync(int routeId, TeamDeleteDto dto)
    {
        if (routeId != dto.TeamId)
        {
            return Result.ValidationFailure(new[]
            {
                new ValidationErrorModel(
                    nameof(dto.TeamId),
                    "Route ID and payload ID must match.")
            });
        }

        var entity = await _dbContext.Teams
            .FirstOrDefaultAsync(t => t.TeamId == routeId);

        if (entity is null)
        {
            return Result.NotFound(
                $"Team with id '{routeId}' not found.");
        }

        byte[] incomingRowVersion;

        try
        {
            incomingRowVersion =
                RowVersionHelper.FromBase64(dto.RowVersion);
        }
        catch
        {
            return Result.ValidationFailure(new[]
            {
                new ValidationErrorModel(
                    nameof(dto.RowVersion),
                    "Invalid RowVersion format.")
            });
        }

        _dbContext.Entry(entity)
            .Property(e => e.RowVersion)
            .OriginalValue = incomingRowVersion;

        _dbContext.Teams.Remove(entity);

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            return Result.Conflict(
                "The record was modified by another user.");
        }

        _logger.LogInformation(
            "Team deleted successfully. TeamId: {TeamId}",
            routeId);

        return Result.Success();
    }
}