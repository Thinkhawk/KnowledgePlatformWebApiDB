using System.ComponentModel.DataAnnotations;

namespace KnowledgePlatformWebApiDB.DtoModels.Teams;

public sealed record class TeamCreateDto
(
    [Required]
    int ProjectId,

    [Required(ErrorMessage = "Team name is required.")]
    [StringLength(
        maximumLength: 100,
        MinimumLength = 2,
        ErrorMessage = "Team name must be between 2 and 100 characters.")]
    string Name,

    [Required]
    string CreatorId
);