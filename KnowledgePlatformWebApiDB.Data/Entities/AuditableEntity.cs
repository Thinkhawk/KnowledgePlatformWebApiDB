using System.ComponentModel.DataAnnotations;

namespace KnowledgePlatformWebApiDB.Data.Entities;

public abstract class AuditableEntity
{

    public DateTime CreatedAtUtc { get; set; }
  
    public DateTime? UpdatedAtUtc { get; set; }


    [Timestamp]
    public byte[] RowVersion { get; set; } = default!;

}
