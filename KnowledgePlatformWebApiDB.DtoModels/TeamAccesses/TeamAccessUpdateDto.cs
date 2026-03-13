using KnowledgePlatformWebApiDB.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace KnowledgePlatformWebApiDB.DtoModels.TeamAccesses;

public sealed record class TeamAccessUpdateDto
(
    [Required]
    int AccessId,

    [Required]
    Level AccessLevel,

    [Required]
    string RowVersion
);