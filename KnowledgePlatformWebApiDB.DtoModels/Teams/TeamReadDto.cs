namespace KnowledgePlatformWebApiDB.DtoModels.Teams;

public sealed record class TeamReadDto
(
    int TeamId,
    int ProjectId,
    string Name,
    string CreatorId,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    string RowVersion
);