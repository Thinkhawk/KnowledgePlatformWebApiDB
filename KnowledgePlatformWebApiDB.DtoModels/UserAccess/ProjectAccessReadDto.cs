namespace KnowledgePlatformWebApiDB.DtoModels.UserAccess;

public sealed record ProjectAccessReadDto(
    int ProjectId,
    string ProjectName,
    string? Description,
    bool IsProjectCreator,
    bool HasFullProjectControl,
    IReadOnlyList<TeamAccessInfoDto> Teams
);
