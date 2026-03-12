using System.ComponentModel.DataAnnotations;

namespace KnowledgePlatformWebApiDB.DtoModels.TeamAccesses;

public sealed record class TeamAccessUpdateDto
(
    [Required]
    int AccessId,

    [Required]
    string AccessLevel
);