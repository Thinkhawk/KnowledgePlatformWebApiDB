using System;
using System.Collections.Generic;
using System.Text;

namespace KnowledgePlatformWebApiDB.DtoModels.Notes;

public sealed record class NoteReadDto(
    Guid NoteId,
    string Title,
    string Content,
    List<string>? Tags,
    int TeamId,
    string CreatorId,
    string CreatorName,
    string UpdaterId,
    string? UpdaterName,
    DateTime CreatedAtUtc,
    DateTime? UpdatedAtUtc,
    string RowVersion
);