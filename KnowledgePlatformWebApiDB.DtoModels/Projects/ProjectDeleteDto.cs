using System.ComponentModel.DataAnnotations;

namespace KnowledgePlatformWebApiDB.DtoModels.Projects;

public sealed record class ProjectDeleteDto
(
    [Required]
    int ProjectId,

    [Required]
    string RowVersion
);