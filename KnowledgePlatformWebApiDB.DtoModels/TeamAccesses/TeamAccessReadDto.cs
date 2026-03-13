using KnowledgePlatformWebApiDB.Data.Enums;

namespace KnowledgePlatformWebApiDB.DtoModels.TeamAccessDtos;

public sealed record class TeamAccessReadDto
(
    int AccessId,

    int TeamId,

    string UserId,

    Level AccessLevel,

    DateTime CreatedAtUtc
);