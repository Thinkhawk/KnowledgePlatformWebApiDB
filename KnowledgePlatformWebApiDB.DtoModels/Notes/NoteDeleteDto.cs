using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace KnowledgePlatformWebApiDB.DtoModels.Notes;

public sealed record class NoteDeleteDto
(
   [Required]
   Guid NoteId,

   [Required]
   string RowVersion
);
