using KnowledgePlatformWebApiDB.Data.Enums;

namespace KnowledgePlatformWebApiDB.DtoModels.TeamAccessDtos;

public sealed record class TeamAccessReadDto
(
    int AccessId,
    int TeamId,
    string UserId,
    string? UserName,
    string? Email,
    Level AccessLevel,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    string RowVersion
);