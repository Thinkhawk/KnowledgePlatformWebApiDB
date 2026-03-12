using Microsoft.AspNetCore.Identity;

public class ApplicationUser : IdentityUser<string>
{
    public string FullName { get; set; } = string.Empty;

}