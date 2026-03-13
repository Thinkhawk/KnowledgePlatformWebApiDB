using System.ComponentModel.DataAnnotations;

namespace KnowledgePlatformWebApiDB.DtoModels.TeamAccesses;

public sealed record class TeamAccessDeleteDto
(
    [Required]
    int AccessId,

    [Required]
    string RowVersion
);