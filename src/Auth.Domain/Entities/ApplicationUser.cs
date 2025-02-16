using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Auth.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        // Personal Information
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Avatar { get; set; }
        public string? Address { get; set; }
        public string? PhoneNumber { get; set; }
        
        // Account Status
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastLoginAt { get; set; }
        public string? LastLoginIp { get; set; }
        public bool IsDeleted { get; set; }
        public DateTime? DeletedAt { get; set; }
        
        // Email Verification
        public string? VerificationToken { get; set; }
        public DateTime? VerificationTokenExpiryTime { get; set; }
        
        // Password Reset
        public string? PasswordResetToken { get; set; }
        public DateTime? PasswordResetTokenExpiryTime { get; set; }
        
        // Session Management
        public int MaxAllowedSessions { get; set; } = 5;  // Maximum concurrent sessions
        public virtual ICollection<UserSession> UserSessions { get; set; }
        
        // Navigation Properties
        public virtual ICollection<UserRole> UserRoles { get; set; }

        // Helper Properties
        public string FullName => $"{FirstName} {LastName}".Trim();
        
        // Account Status Methods
        public bool IsLocked => LockoutEnd.HasValue && LockoutEnd > DateTime.UtcNow;
        public bool CanLogin => IsActive && !IsDeleted && !IsLocked;
        
        // Helper Methods
        public bool HasActiveSession(string deviceId)
            => UserSessions?.Any(s => s.DeviceId == deviceId && 
                                !s.IsRevoked && 
                                s.ExpiresAt > DateTime.UtcNow) ?? false;
                            
        public bool HasReachedMaxSessions()
            => UserSessions?.Count(s => !s.IsRevoked && 
                                   s.ExpiresAt > DateTime.UtcNow) >= MaxAllowedSessions;
    }
}
