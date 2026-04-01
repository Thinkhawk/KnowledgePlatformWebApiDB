using KnowledgePlatformWebApiDB.Data.Data;
using KnowledgePlatformWebApiDB.Data.Entities;
using KnowledgePlatformWebApiDB.DtoModels.TeamAccessDtos;
using KnowledgePlatformWebApiDB.DtoModels.TeamAccesses;
using KnowledgePlatformWebApiDB.Infrastructure.Helpers;
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
            AccessLevel = dto.AccessLevel
        };

        _dbContext.TeamAccesses.Add(entity);

        await _dbContext.SaveChangesAsync();

        var location = $"/api/teamaccess/{entity.AccessId}";

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
        var result = await _dbContext.TeamAccesses
            .AsNoTracking()
            .Where(a => a.AccessId == id)
            .Join(_dbContext.Users,
                a => a.UserId,
                u => u.Id,
                (a, u) => new { Access = a, User = u })
            .FirstOrDefaultAsync();

        if (result is null)
        {
            return Result<TeamAccessReadDto>.NotFound(
                $"TeamAccess with id '{id}' not found.");
        }

        var dto = new TeamAccessReadDto(
            AccessId: result.Access.AccessId,
            TeamId: result.Access.TeamId,
            UserId: result.Access.UserId,
            UserName: result.User.UserName,
            Email: result.User.Email,
            AccessLevel: result.Access.AccessLevel,
            CreatedAtUtc: result.Access.CreatedAtUtc,
            UpdatedAtUtc: result.Access.UpdatedAtUtc,
            RowVersion: RowVersionHelper.ToBase64(result.Access.RowVersion)
        );

        return Result<TeamAccessReadDto>.Success(dto);
    }


    /// <summary>
    /// Get all TeamAccess records, optionally filtered by teamId
    /// </summary>
    public async Task<Result<IReadOnlyList<TeamAccessReadDto>>> ReadAllAsync(int? teamId = null)
    {
        var query = _dbContext.TeamAccesses
            .AsNoTracking()
            .Where(a => !teamId.HasValue || a.TeamId == teamId.Value)
            .Join(_dbContext.Users,
                a => a.UserId,
                u => u.Id,
                (a, u) => new { Access = a, User = u })
            .OrderBy(x => x.Access.AccessId);

        var rows = await query.ToListAsync();

        var dtos = rows
            .Select(x => new TeamAccessReadDto(
                x.Access.AccessId,
                x.Access.TeamId,
                x.Access.UserId,
                UserName: x.User.UserName,
                Email: x.User.Email,
                x.Access.AccessLevel,
                x.Access.CreatedAtUtc,
                x.Access.UpdatedAtUtc,
                RowVersion: RowVersionHelper.ToBase64(x.Access.RowVersion)
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

        byte[] incomingRowVersion;
        try
        {
            incomingRowVersion = RowVersionHelper.FromBase64(dto.RowVersion);
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

        entity.AccessLevel = dto.AccessLevel;

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

        byte[] incomingRowVersion;
        try
        {
            incomingRowVersion = RowVersionHelper.FromBase64(dto.RowVersion);
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

        _dbContext.TeamAccesses.Remove(entity);

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
            "TeamAccess deleted successfully. AccessId: {AccessId}",
            routeId);

        return Result.Success();
    }
}