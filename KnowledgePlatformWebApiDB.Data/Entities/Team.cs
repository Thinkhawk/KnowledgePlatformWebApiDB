using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgePlatformWebApiDB.Data.Entities;

[Table("Teams", Schema = "dbo")]
[Index(nameof(ProjectId), Name = "IX_Teams_ProjectId")]
[Index(nameof(ProjectId), nameof(Name), IsUnique = true, Name = "UQ_Project_Team")]
public sealed class Team : AuditableEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int TeamId { get; set; }

    [Required(ErrorMessage = "{0} cannot be empty.")]
    [MaxLength(100)]
    [Column(TypeName = "varchar(100)")]
    public string Name { get; set; } = string.Empty;


    // -------- Relationship with Project (many → 1)

    [Required]
    public int ProjectId { get; set; }

    [ForeignKey(nameof(ProjectId))]
    public Project Project { get; set; } = null!;


    // -------- Relationship with ApplicationUser

    [Required]
    public string CreatorId { get; set; } = default!;

    [ForeignKey(nameof(CreatorId))]
    public ApplicationUser User { get; set; } = null!;


    // -------- Navigation Properties

    public ICollection<TeamAccess> TeamAccesses { get; set; } = new List<TeamAccess>();

    public ICollection<Note> Note { get; set; } = new List<Note>();
}