using System;
using System.Collections.Generic;
using System.Text;

namespace KnowledgePlatformWebApiDB.DtoModels.Notes;

public sealed record class NoteFilterDto(
    Guid? NoteId,
    string? Title,
    List<string>? Tags,
    string? CreatorId,
    string? UpdaterId,
    DateTime? CreatedAtUtc,
    DateTime? UpdatedAtUtc
);
