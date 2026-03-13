using KnowledgePlatformWebApiDB.Data.Enums;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgePlatformWebApiDB.Data.Entities;

[Table("TeamAccess", Schema = "dbo")]
[Index(nameof(UserId), nameof(TeamId), IsUnique = true, Name = "UQ_User_Team")]
public sealed class TeamAccess : AuditableEntity
{
    /// Primary Key
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int AccessId { get; set; }

    /// User who has access
    [Required]
    public string UserId { get; set; } = string.Empty;

    /// Access level (Read / Write)
    [Required]
    [MaxLength(10)]
    [Column(TypeName = "varchar(10)")]
    public Level AccessLevel { get; set; } = Level.Read;


    // -------- Relationship with Team

    [Required]
    public int TeamId { get; set; }


    [ForeignKey(nameof(TeamId))]
    public Team Team { get; set; } = null!;
}