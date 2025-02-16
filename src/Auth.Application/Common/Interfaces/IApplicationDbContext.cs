using Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Auth.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<ApplicationUser> Users { get; set; }
        DbSet<Role> Tokens { get; set; }
        DbSet<UserRole> UserRoles { get; set; }
        DbSet<UserSession> UserSessions { get; set; }
    }
}

