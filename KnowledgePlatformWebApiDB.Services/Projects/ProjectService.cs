using KnowledgePlatformWebApiDB.Data.Data;
using KnowledgePlatformWebApiDB.Data.Entities;
using KnowledgePlatformWebApiDB.DtoModels.Projects;
using KnowledgePlatformWebApiDB.Infrastructure.Helpers;
using KnowledgePlatformWebApiDB.Infrastructure.Results;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KnowledgePlatformWebApiDB.Services.Projects;

public sealed class ProjectService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<ProjectService> _logger;

    public ProjectService(
        ApplicationDbContext db,
        ILogger<ProjectService> logger)
    {
        _dbContext = db;
        _logger = logger;
    }

    public async Task<Result<string>> CreateAsync(ProjectCreateDto dto)
    {
        var entity = new Project
        {
            Name = dto.Name,
            Description = dto.Description,
            CreatedAtUtc = DateTime.UtcNow
        };

        _dbContext.Projects.Add(entity);

        await _dbContext.SaveChangesAsync();

        var location = $"/api/projects/{entity.ProjectId}";

        return Result<string>.Created(location);
    }

    public async Task<Result<ProjectReadDto>> ReadOneAsync(int id)
    {
        var entity = await _dbContext.Projects
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ProjectId == id);

        if (entity is null)
        {
            return Result<ProjectReadDto>.NotFound(
                $"Project with id '{id}' not found.");
        }

        var dto = new ProjectReadDto(
            entity.ProjectId,
            entity.Name,
            entity.Description,
            entity.CreatedAtUtc,
            entity.UpdatedAtUtc,
            RowVersionHelper.ToBase64(entity.RowVersion)
        );

        return Result<ProjectReadDto>.Success(dto);
    }

    public async Task<Result<IReadOnlyList<ProjectReadDto>>> ReadAllAsync()
    {
        var entities = await _dbContext.Projects
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();

        var dtos = entities
            .Select(e => new ProjectReadDto(
                e.ProjectId,
                e.Name,
                e.Description,
                e.CreatedAtUtc,
                e.UpdatedAtUtc,
                RowVersionHelper.ToBase64(e.RowVersion)
            ))
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<ProjectReadDto>>.Success(dtos);
    }

    public async Task<Result> UpdateAsync(int routeId, ProjectUpdateDto dto)
    {
        var entity = await _dbContext.Projects
            .FirstOrDefaultAsync(p => p.ProjectId == routeId);

        if (entity is null)
        {
            return Result.NotFound($"Project with id '{routeId}' not found.");
        }

        entity.Name = dto.Name;
        entity.Description = dto.Description;

        await _dbContext.SaveChangesAsync();

        return Result.Accepted();
    }

    public async Task<Result> DeleteAsync(int routeId, ProjectDeleteDto dto)
    {
        var entity = await _dbContext.Projects
            .FirstOrDefaultAsync(p => p.ProjectId == routeId);

        if (entity is null)
        {
            return Result.NotFound($"Project with id '{routeId}' not found.");
        }

        _dbContext.Projects.Remove(entity);

        await _dbContext.SaveChangesAsync();

        return Result.Success();
    }
}