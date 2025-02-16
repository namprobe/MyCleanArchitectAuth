using Auth.Domain.Common;

namespace Auth.Domain.Entities;

public class UserSession : BaseEntity
{
    public string UserId { get; set; }
    public string RefreshToken { get; set; }
    public string DeviceId { get; set; }
    public string DeviceName { get; set; }
    public string IpAddress { get; set; }
    public string UserAgent { get; set; }
    public DateTime LastActivity { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsRevoked { get; set; }
    public DateTime? RevokedOn { get; set; }
    
    // Navigation property
    public virtual ApplicationUser User { get; set; }
}