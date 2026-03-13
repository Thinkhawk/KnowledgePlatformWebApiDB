using KnowledgePlatformWebApiDB.Data.Data;
using KnowledgePlatformWebApiDB.Data.Enums;
using KnowledgePlatformWebApiDB.DtoModels.UserAccess;
using KnowledgePlatformWebApiDB.Infrastructure.Results;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace KnowledgePlatformWebApiDB.Services.UserAccess;

public sealed class UserAccessService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<UserAccessService> _logger;

    public UserAccessService(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        ILogger<UserAccessService> logger)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves all projects and teams a user has access to, enforcing role-based 
    /// and ownership-based write permissions.
    /// </summary>
    public async Task<Result<IReadOnlyList<ProjectAccessReadDto>>> GetUserAccessibleProjectsAndTeamsAsync(string userId)
    {
        // 1. Validation: Ensure userId is provided
        if (string.IsNullOrWhiteSpace(userId))
        {
            return Result<IReadOnlyList<ProjectAccessReadDto>>.ValidationFailure(new[]
            {
                new ValidationErrorModel(nameof(userId), "User ID cannot be empty.")
            });
        }

        // 2. Validation: Ensure user exists and resolve Roles
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
        {
            _logger.LogWarning("Access retrieval failed: User {UserId} not found.", userId);
            return Result<IReadOnlyList<ProjectAccessReadDto>>.NotFound($"User with ID '{userId}' not found.");
        }

        bool isProjectAdmin = await _userManager.IsInRoleAsync(user, "ProjectAdmin"); //
        bool isProjectLead = await _userManager.IsInRoleAsync(user, "ProjectLead");   //

        // 3. Query Logic: Aggregate projects and teams
        var query = _dbContext.Projects.AsNoTracking();

        // If NOT an Admin, filter by projects they are involved in
        if (!isProjectAdmin)
        {
            query = query.Where(p =>
                p.CreatorId == userId ||
                p.Teams.Any(t => t.CreatorId == userId ||
                                 t.TeamAccesses.Any(ta => ta.UserId == userId)));
        }

        var projectData = await query
            .Select(p => new
            {
                p.ProjectId,
                p.Name,
                p.Description,
                p.CreatorId,
                // Check if a ProjectLead is "added" to this project via any team access record
                IsAddedAsLead = isProjectLead && p.Teams.Any(t => t.TeamAccesses.Any(ta => ta.UserId == userId)),
                Teams = p.Teams.Select(t => new
                {
                    t.TeamId,
                    t.Name,
                    t.CreatorId,
                    // Fetch specific access level if it exists for this team
                    ExplicitAccess = t.TeamAccesses
                        .Where(ta => ta.UserId == userId)
                        .Select(ta => (Level?)ta.AccessLevel)
                        .FirstOrDefault()
                }).ToList()
            })
            .OrderBy(p => p.Name)
            .ToListAsync();

        // 4. Mapping and Rule Enforcement
        var results = projectData.Select(p =>
        {

            // RULE: Admins, Project Creators, and Leads added to the project get Full Control (Write access)
            bool hasFullProjectControl = isProjectAdmin || p.CreatorId == userId || p.IsAddedAsLead;

            return new ProjectAccessReadDto(
                ProjectId: p.ProjectId,
                ProjectName: p.Name,
                Description: p.Description,
                IsProjectCreator: p.CreatorId == userId,
                HasFullProjectControl: hasFullProjectControl, // UI uses this to grant project-wide write 
                Teams: p.Teams
                    .Where(t => hasFullProjectControl ||
                                t.CreatorId == userId ||
                                t.ExplicitAccess.HasValue)
                    .Select(t => new TeamAccessInfoDto(
                        TeamId: t.TeamId,
                        TeamName: t.Name,
                        // RULE: If they have Full Project Control or created the team, they get Level.Write
                        AccessLevel: (hasFullProjectControl || t.CreatorId == userId)
                            ? Level.Write
                            : t.ExplicitAccess!.Value,
                        IsTeamCreator: t.CreatorId == userId
                    ))
                    .ToList()
            );
        }).ToList();

        _logger.LogInformation("Retrieved {Count} accessible projects for user {UserId}.", results.Count, userId);

        return Result<IReadOnlyList<ProjectAccessReadDto>>.Success(results);
    }
}