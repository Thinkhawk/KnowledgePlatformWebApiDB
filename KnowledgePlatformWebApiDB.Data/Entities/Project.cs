using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgePlatformWebApiDB.Data.Entities;

[Table("Projects", Schema = "dbo")]
[Index(nameof(Name), IsUnique = true, Name = "UQ_Projects_Name")]
public sealed class Project : AuditableEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int ProjectId { get; set; }

    [Required(ErrorMessage = "{0} cannot be empty.")]
    [MaxLength(150)]
    [Column(TypeName = "varchar(150)")]
    public string Name { get; set; } = string.Empty;

    [MaxLength(500)]
    public string? Description { get; set; }

    // -------- Relationship with ApplicationUser

    [Required]
    public string CreatorId { get; set; } = string.Empty;

    [ForeignKey(nameof(CreatorId))]
    public ApplicationUser User { get; set; } = null!;


    // ------- Navigation Property
    public ICollection<Team> Teams { get; set; } = new List<Team>();
}