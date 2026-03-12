using System.ComponentModel.DataAnnotations;

namespace KnowledgePlatformWebApiDB.DtoModels.Teams;

public sealed record class TeamDeleteDto
(
    [Required]
    int TeamId,

    [Required]
    string RowVersion
);