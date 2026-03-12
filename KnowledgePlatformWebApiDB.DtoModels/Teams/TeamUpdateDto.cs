using System.ComponentModel.DataAnnotations;

namespace KnowledgePlatformWebApiDB.DtoModels.Teams;

public sealed record class TeamUpdateDto
(
    [Required]
    int TeamId,

    [Required(ErrorMessage = "Team name is required.")]
    [StringLength(
        maximumLength: 100,
        MinimumLength = 2)]
    string Name,

    [Required]
    string RowVersion
);