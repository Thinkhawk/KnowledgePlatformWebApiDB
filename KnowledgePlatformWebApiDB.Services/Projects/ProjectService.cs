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
    // Database context used to access database tables
    private readonly ApplicationDbContext _dbContext;

    // Logger used to record warnings, errors, and info
    private readonly ILogger<ProjectService> _logger;

    // Constructor dependency injection
    // ASP.NET automatically injects DbContext and Logger
    public ProjectService(
        ApplicationDbContext db,
        ILogger<ProjectService> logger)
    {
        _dbContext = db;
        _logger = logger;
    }

    // ------------------------------------------------------------
    // CREATE PROJECT
    // ------------------------------------------------------------
    public async Task<Result<string>> CreateAsync(ProjectCreateDto dto)
    {
        // Trim whitespace from project name
        var name = dto.Name?.Trim();

        // Validation: project name should not be empty
        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogWarning("Project creation failed: Name is empty.");

            return Result<string>.ValidationFailure(new[]
            {
                new ValidationErrorModel(nameof(dto.Name), "Project name cannot be empty.")
            });
        }

        // Validation: prevent duplicate project names
        bool duplicateExists = await _dbContext.Projects
            .AnyAsync(p => p.Name.ToUpper() == name.ToUpper());

        if (duplicateExists)
        {
            _logger.LogWarning("Project creation failed: Duplicate project '{ProjectName}' attempted.", name);

            return Result<string>.ValidationFailure(new[]
            {
                new ValidationErrorModel(nameof(dto.Name), $"Project '{name}' already exists.")
            });
        }

        // Create project entity from DTO
        var entity = new Project
        {
            Name = name,
            Description = dto.Description,
            CreatorId = dto.CreatorId
        };

        // Add to database
        _dbContext.Projects.Add(entity);

        // Save changes to database
        await _dbContext.SaveChangesAsync();

        // Location of created resource
        var location = $"/api/projects/{entity.ProjectId}";

        _logger.LogInformation("Project created successfully. ProjectId: {ProjectId}", entity.ProjectId);

        return Result<string>.Created(location);
    }

    // ------------------------------------------------------------
    // READ SINGLE PROJECT
    // ------------------------------------------------------------
    public async Task<Result<ProjectReadDto>> ReadOneAsync(int id)
    {
        // Find project by ID
        var entity = await _dbContext.Projects
            .AsNoTracking() // Improves performance because tracking is not needed
            .FirstOrDefaultAsync(p => p.ProjectId == id);

        // If project not found return NotFound
        if (entity is null)
        {
            _logger.LogWarning("Project retrieval failed: ProjectId {ProjectId} not found.", id);

            return Result<ProjectReadDto>.NotFound($"Project with id '{id}' not found.");
        }

        // Convert entity into DTO
        var dto = new ProjectReadDto(
            entity.ProjectId,
            entity.Name,
            entity.Description,
            entity.CreatorId,
            entity.CreatedAtUtc,
            entity.UpdatedAtUtc,
            RowVersionHelper.ToBase64(entity.RowVersion) // Used for concurrency control
        );

        _logger.LogInformation("Project retrieved successfully. ProjectId: {ProjectId}", id);

        return Result<ProjectReadDto>.Success(dto);
    }

    // ------------------------------------------------------------
    // READ ALL PROJECTS
    // ------------------------------------------------------------
    public async Task<Result<IReadOnlyList<ProjectReadDto>>> ReadAllAsync()
    {
        // Retrieve all projects ordered by name
        var entities = await _dbContext.Projects
            .AsNoTracking()
            .OrderBy(p => p.Name)
            .ToListAsync();

        // Convert database entities into DTO objects
        var dtos = entities
            .Select(e => new ProjectReadDto(
                e.ProjectId,
                e.Name,
                e.Description,
                e.CreatorId,
                e.CreatedAtUtc,
                e.UpdatedAtUtc,
                RowVersionHelper.ToBase64(e.RowVersion)
            ))
            .ToList()
            .AsReadOnly();

        _logger.LogInformation("Retrieved {Count} projects.", dtos.Count);

        return Result<IReadOnlyList<ProjectReadDto>>.Success(dtos);
    }

    // ------------------------------------------------------------
    // UPDATE PROJECT
    // ------------------------------------------------------------
    public async Task<Result> UpdateAsync(int routeId, ProjectUpdateDto dto)
    {
        // Validation: route ID and payload ID must match
        if (routeId != dto.ProjectId)
        {
            _logger.LogWarning("Project update failed: RouteId {RouteId} does not match PayloadId {PayloadId}.",
                routeId, dto.ProjectId);

            return Result.ValidationFailure(new[]
            {
                new ValidationErrorModel(nameof(dto.ProjectId),
                $"Route ProjectId {routeId} and payload ProjectId {dto.ProjectId} must match.")
            });
        }

        // Find project in database
        var entity = await _dbContext.Projects
            .FirstOrDefaultAsync(p => p.ProjectId == routeId);

        // If project does not exist
        if (entity is null)
        {
            _logger.LogWarning("Project update failed: ProjectId {ProjectId} not found.", routeId);

            return Result.NotFound($"Project with id '{routeId}' not found.");
        }

        // Convert row version string back to byte[]
        byte[] incomingRowVersion;

        try
        {
            incomingRowVersion = RowVersionHelper.FromBase64(dto.RowVersion);
        }
        catch (ArgumentException)
        {
            _logger.LogWarning("Project update failed due to invalid RowVersion format. ProjectId: {ProjectId}", routeId);

            return Result.ValidationFailure(new[]
            {
                new ValidationErrorModel(nameof(dto.RowVersion), "Invalid RowVersion format.")
            });
        }

        // Apply concurrency check
        _dbContext.Entry(entity)
            .Property(p => p.RowVersion)
            .OriginalValue = incomingRowVersion;

        // Trim project name
        var name = dto.Name?.Trim();

        // Validate name is not empty
        if (string.IsNullOrWhiteSpace(name))
        {
            _logger.LogWarning("Project update failed: Empty project name provided. ProjectId: {ProjectId}", routeId);

            return Result.ValidationFailure(new[]
            {
                new ValidationErrorModel(nameof(dto.Name), "Project name cannot be empty.")
            });
        }

        // Check duplicate project name
        bool duplicateExists = await _dbContext.Projects
            .AnyAsync(p => p.Name.ToUpper() == name.ToUpper() && p.ProjectId != routeId);

        if (duplicateExists)
        {
            _logger.LogWarning("Project update failed: Duplicate project name '{ProjectName}' attempted.", name);

            return Result.ValidationFailure(new[]
            {
                new ValidationErrorModel(nameof(dto.Name), $"Project '{name}' already exists.")
            });
        }

        // Update entity values
        entity.Name = name;
        entity.Description = dto.Description;

        try
        {
            // Save update to database
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            // Concurrency conflict if someone updated before you
            _logger.LogWarning("Project update concurrency conflict. ProjectId: {ProjectId}", routeId);

            return Result.Concurrency($"The project with id '{routeId}' was updated by another user.");
        }

        _logger.LogInformation("Project updated successfully. ProjectId: {ProjectId}", routeId);

        return Result.Accepted();
    }

    // ------------------------------------------------------------
    // DELETE PROJECT
    // ------------------------------------------------------------
    public async Task<Result> DeleteAsync(int routeId, ProjectDeleteDto dto)
    {
        // Validation: route and payload ID must match
        if (routeId != dto.ProjectId)
        {
            _logger.LogWarning("Project delete failed: RouteId {RouteId} does not match PayloadId {PayloadId}.",
                routeId, dto.ProjectId);

            return Result.ValidationFailure(new[]
            {
                new ValidationErrorModel(nameof(dto.ProjectId),
                $"Route ProjectId {routeId} and payload ProjectId {dto.ProjectId} must match.")
            });
        }

        // Find project in database
        var entity = await _dbContext.Projects
            .FirstOrDefaultAsync(p => p.ProjectId == routeId);

        if (entity is null)
        {
            _logger.LogWarning("Project delete failed: ProjectId {ProjectId} not found.", routeId);

            return Result.NotFound($"Project with id '{routeId}' not found.");
        }

        // Convert row version
        byte[] incomingRowVersion;

        try
        {
            incomingRowVersion = RowVersionHelper.FromBase64(dto.RowVersion);
        }
        catch (ArgumentException)
        {
            return Result.ValidationFailure(new[]
            {
                new ValidationErrorModel(nameof(dto.RowVersion), "Invalid RowVersion format.")
            });
        }

        // Apply concurrency validation
        _dbContext.Entry(entity)
            .Property(p => p.RowVersion)
            .OriginalValue = incomingRowVersion;

        // Remove project
        _dbContext.Projects.Remove(entity);

        try
        {
            await _dbContext.SaveChangesAsync();
        }
        catch (Exception)
        {
            _logger.LogWarning("Project delete concurrency conflict. ProjectId: {ProjectId}", routeId);

            return Result.Concurrency($"The project with id '{routeId}' was updated by another user.");
        }

        _logger.LogInformation("Project deleted successfully. ProjectId: {ProjectId}", routeId);

        return Result.Success();
    }
}