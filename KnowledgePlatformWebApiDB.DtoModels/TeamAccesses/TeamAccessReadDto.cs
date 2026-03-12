namespace KnowledgePlatformWebApiDB.DtoModels.TeamAccessDtos;

public sealed record class TeamAccessReadDto
(
    int AccessId,

    int TeamId,

    string UserId,

    string AccessLevel,

    DateTime CreatedAtUtc
);