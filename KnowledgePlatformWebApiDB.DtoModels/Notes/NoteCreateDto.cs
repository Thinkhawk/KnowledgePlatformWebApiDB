using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KnowledgePlatformWebApiDB.Data.Entities;

[Table("Notes")]
public sealed record class NoteCreateDto
(

    [Display(Name = "Tile of the Note")]
    [Required(ErrorMessage = "{0} cannot be empty.")]
    [StringLength(maximumLength: 100, MinimumLength = 3, ErrorMessage = "{0} should have characteres b/w {1} and {2} characters")]
    string Title,


    [Display(Name = "Content of the Note")]
    [Required(ErrorMessage = "{0} cannot be empty.")]
    string Content,


    [Display(Name = "Tags of the Note")]
    List<string> Tags,


    [Required]
    int TeamId,


    [Required]
    string UserId

);
