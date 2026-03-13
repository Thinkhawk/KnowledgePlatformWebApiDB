using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgePlatformWebApiDB.Data.Entities;

[Table("Notes", Schema = "dbo")]
public sealed class Note
    : AuditableEntity
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid NoteId { get; set; }


    [Display(Name = "Tile of the Note")]
    [Required(ErrorMessage = "{0} cannot be empty.")]
    [StringLength(maximumLength: 100, MinimumLength = 3, ErrorMessage = "{0} should have characters b/w {1} and {2} characters")]
    public string? Title { get; set; }


    [Display(Name = "Content of the Note")]
    [Required(ErrorMessage = "{0} cannot be empty.")]
    public string? Content { get; set; }


    [Display(Name = "Tags of the Note")]
    public List<string> Tags { get; set; } = [];


    // -------- Relationship with Team

    [Required]
    public int TeamId { get; set; }


    [ForeignKey(nameof(TeamId))]
    public Team? Team { get; set; }


    // -------- Relationship with ApplicationUser

    [Required]
    public string UserId { get; set; } = default!;


    [ForeignKey(nameof(UserId))]
    public ApplicationUser User { get; set; } = null!;
}
