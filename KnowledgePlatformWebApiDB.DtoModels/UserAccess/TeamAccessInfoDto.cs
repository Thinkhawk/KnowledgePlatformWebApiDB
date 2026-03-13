using KnowledgePlatformWebApiDB.Data.Enums;

namespace KnowledgePlatformWebApiDB.DtoModels.UserAccess;

public sealed record TeamAccessInfoDto(
    int TeamId,
    string TeamName,
    Level AccessLevel, // From KnowledgePlatformWebApiDB.Data.Enums
    bool IsTeamCreator
);
