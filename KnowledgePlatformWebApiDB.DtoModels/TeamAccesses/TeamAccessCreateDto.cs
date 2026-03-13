using System.ComponentModel.DataAnnotations;

using KnowledgePlatformWebApiDB.Data.Enums;

namespace KnowledgePlatformWebApiDB.DtoModels.TeamAccessDtos;

public sealed record class TeamAccessCreateDto
(
    [Required]
    int TeamId,

    [Required]
    string UserId,

    [Required]
    Level AccessLevel
);