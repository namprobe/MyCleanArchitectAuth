using Microsoft.AspNetCore.Identity;

namespace Auth.Domain.Entities;
public class UserRole : IdentityUserRole<string>
{
    // Navigation properties
    public virtual ApplicationUser User { get; set; }
    public virtual Role Role { get; set; }
}