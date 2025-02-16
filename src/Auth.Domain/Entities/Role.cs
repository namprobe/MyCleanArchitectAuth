namespace Auth.Domain.Entities;

using Microsoft.AspNetCore.Identity;
public class Role : IdentityRole
{
    public string? Description { get; set; }
    
    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; }
}