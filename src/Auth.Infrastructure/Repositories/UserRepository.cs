using System.Linq.Expressions;
using Auth.Application.Common.Interfaces;
using Auth.Domain.Entities;
using Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

public class UserRepository : GenericRepository<ApplicationUser>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public async Task<ApplicationUser?> GetUserByIdAsync(string id, params Expression<Func<ApplicationUser, object>>[] includes)
    {
        return await GetByIdAsync(id, includes);
    }

    public async Task<ApplicationUser> GetUserByEmailAsync(string email)
    {
        return await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<IEnumerable<UserSession>> GetUserSessionsAsync(string userId)
    {
        return await _context.UserSessions.Where(x => x.UserId == userId).ToListAsync();
    }

    public async Task<bool> IsEmailUniqueAsync(string email)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        return user == null;
    }

    public async Task<ApplicationUser?> GetByVerificationTokenAsync(string token)
    {
        return await GetFirstOrDefaultAsync(x => x.VerificationToken == token);
    }
}
