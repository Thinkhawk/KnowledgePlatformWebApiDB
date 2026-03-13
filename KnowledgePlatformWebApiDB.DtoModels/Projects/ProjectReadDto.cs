namespace KnowledgePlatformWebApiDB.DtoModels.Projects;

public sealed record class ProjectReadDto
(
    int ProjectId,

    string Name,

    string? Description,

    string CreatorId,

    DateTime CreatedAtUtc,

    DateTime? UpdatedAtUtc,

    string RowVersion
);