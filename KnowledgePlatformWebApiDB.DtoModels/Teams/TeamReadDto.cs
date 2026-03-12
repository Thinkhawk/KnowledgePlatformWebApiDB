namespace KnowledgePlatformWebApiDB.DtoModels.Teams;

public sealed record class TeamReadDto
(
    int TeamId,

    int ProjectId,

    string Name,

    DateTime CreatedAtUtc,

    DateTime? ModifiedAtUtc,

    string RowVersion
);