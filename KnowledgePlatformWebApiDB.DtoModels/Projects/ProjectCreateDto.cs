using System.ComponentModel.DataAnnotations;

namespace KnowledgePlatformWebApiDB.DtoModels.Projects;

public sealed record class ProjectCreateDto
(
    [Required(ErrorMessage = "Project name is required.")]
    [StringLength(
        maximumLength: 100,
        MinimumLength = 2,
        ErrorMessage = "Project name must be between 2 and 100 characters.")]
    string Name,

    [StringLength(
        maximumLength: 4000,
        ErrorMessage = "Description cannot exceed 4000 characters.")]
    string? Description,

    [Required]
    string CreatorId
);