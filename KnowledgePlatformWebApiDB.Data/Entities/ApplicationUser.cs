using KnowledgePlatformWebApiDB.Data.Entities;
using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public ICollection<Project> Projects { get; set; } = new List<Project>();
    public ICollection<Team> Teams { get; set; } = new List<Team>();
    public ICollection<Note> Notes { get; set; } = new List<Note>();

}