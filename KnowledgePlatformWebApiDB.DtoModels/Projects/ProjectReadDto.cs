namespace KnowledgePlatformWebApiDB.DtoModels.Projects;

public sealed record class ProjectReadDto
(
    int ProjectId,

    string Name,

    string? Description,

    DateTime CreatedAtUtc,

    DateTime? ModifiedAtUtc,

    string RowVersion
);