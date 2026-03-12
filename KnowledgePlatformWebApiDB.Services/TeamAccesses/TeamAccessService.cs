using KnowledgePlatformWebApiDB.Data.Data;
using KnowledgePlatformWebApiDB.Data.Entities;
using KnowledgePlatformWebApiDB.DtoModels.TeamAccessDtos;
using KnowledgePlatformWebApiDB.DtoModels.TeamAccesses;
using KnowledgePlatformWebApiDB.Infrastructure.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KnowledgePlatformWebApiDB.Services.TeamAccesses;

public sealed class TeamAccessService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<TeamAccessService> _logger;

    public TeamAccessService(
        ApplicationDbContext db,
        ILogger<TeamAccessService> logger)
    {
        _dbContext = db;
        _logger = logger;
    }

    /// <summary>
    /// Assign access to a user for a team
    /// </summary>
    public async Task<Result<string>> CreateAsync(TeamAccessCreateDto dto)
    {
        // ----- Validate Team exists
        bool teamExists = await _dbContext.Teams
            .AnyAsync(t => t.TeamId == dto.TeamId);

        if (!teamExists)
        {
            return Result<string>.NotFound(
                $"Team with id '{dto.TeamId}' not found.");
        }

        // ----- Validate User exists
        bool userExists = await _dbContext.Users
            .AnyAsync(u => u.Id == dto.UserId);

        if (!userExists)
        {
            return Result<string>.NotFound(
                $"User with id '{dto.UserId}' not found.");
        }

        // ----- Check duplicate access
        bool alreadyExists = await _dbContext.TeamAccesses
            .AnyAsync(a =>
                a.TeamId == dto.TeamId &&
                a.UserId == dto.UserId);

        if (alreadyExists)
        {
            return Result<string>.Conflict(
                "User already has access to this team.");
        }

        var entity = new TeamAccess
        {
            TeamId = dto.TeamId,
            UserId = dto.UserId,
            AccessLevel = dto.AccessLevel,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.TeamAccesses.Add(entity);

        await _dbContext.SaveChangesAsync();

        var location = $"/api/team access/{entity.AccessId}";

        _logger.LogInformation(
            "TeamAccess created successfully. AccessId: {AccessId}",
            entity.AccessId);

        return Result<string>.Created(location);
    }


    /// <summary>
    /// Get one TeamAccess by Id
    /// </summary>
    public async Task<Result<TeamAccessReadDto>> ReadOneAsync(int id)
    {
        var entity = await _dbContext.TeamAccesses
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AccessId == id);

        if (entity is null)
        {
            return Result<TeamAccessReadDto>.NotFound(
                $"TeamAccess with id '{id}' not found.");
        }

        var dto = new TeamAccessReadDto(
            AccessId: entity.AccessId,
            TeamId: entity.TeamId,
            UserId: entity.UserId,
            AccessLevel: entity.AccessLevel,
            CreatedAtUtc: entity.CreatedAtUtc
        );

        return Result<TeamAccessReadDto>.Success(dto);
    }


    /// <summary>
    /// Get all TeamAccess records
    /// </summary>
    public async Task<Result<IReadOnlyList<TeamAccessReadDto>>> ReadAllAsync()
    {
        var entities = await _dbContext.TeamAccesses
            .AsNoTracking()
            .OrderBy(a => a.AccessId)
            .ToListAsync();

        var dtos = entities
            .Select(e => new TeamAccessReadDto(
                e.AccessId,
                e.TeamId,
                e.UserId,
                e.AccessLevel,
                e.CreatedAtUtc
            ))
            .ToList()
            .AsReadOnly();

        _logger.LogInformation(
            "Retrieved {Count} team access records.",
            dtos.Count);

        return Result<IReadOnlyList<TeamAccessReadDto>>.Success(dtos);
    }


    /// <summary>
    /// Update access level
    /// </summary>
    public async Task<Result> UpdateAsync(int routeId, TeamAccessUpdateDto dto)
    {
        if (routeId != dto.AccessId)
        {
            return Result.ValidationFailure(new[]
            {
                new ValidationErrorModel(
                    nameof(dto.AccessId),
                    "Route ID and payload ID must match.")
            });
        }

        var entity = await _dbContext.TeamAccesses
            .FirstOrDefaultAsync(a => a.AccessId == routeId);

        if (entity is null)
        {
            return Result.NotFound(
                $"TeamAccess with id '{routeId}' not found.");
        }

        entity.AccessLevel = dto.AccessLevel;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "TeamAccess updated successfully. AccessId: {AccessId}",
            routeId);

        return Result.Accepted();
    }


    /// <summary>
    /// Remove access from user
    /// </summary>
    public async Task<Result> DeleteAsync(int routeId, TeamAccessDeleteDto dto)
    {
        if (routeId != dto.AccessId)
        {
            return Result.ValidationFailure(new[]
            {
                new ValidationErrorModel(
                    nameof(dto.AccessId),
                    "Route ID and payload ID must match.")
            });
        }

        var entity = await _dbContext.TeamAccesses
            .FirstOrDefaultAsync(a => a.AccessId == routeId);

        if (entity is null)
        {
            return Result.NotFound(
                $"TeamAccess with id '{routeId}' not found.");
        }

        _dbContext.TeamAccesses.Remove(entity);

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation(
            "TeamAccess deleted successfully. AccessId: {AccessId}",
            routeId);

        return Result.Success();
    }
}