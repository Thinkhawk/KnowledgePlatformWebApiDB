using System.ComponentModel.DataAnnotations;

namespace KnowledgePlatformWebApiDB.DtoModels.TeamAccessDtos;

public sealed record class TeamAccessCreateDto
(
    [Required]
    int TeamId,

    [Required]
    string UserId,

    [Required]
    string AccessLevel
);