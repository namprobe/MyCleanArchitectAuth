using Auth.Application.Common.Interfaces;
using Auth.Domain.Entities;
using Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

public class UserSessionRepository : GenericRepository<UserSession>, IUserSessionRepository
{
    public UserSessionRepository(ApplicationDbContext context) : base(context)
    {
        
    }

    public async Task<IEnumerable<UserSession>> GetActiveSessionsAsync(string userId)
    {
        return await FindByCondition(x => x.UserId == userId && x.IsRevoked == false && x.ExpiresAt > DateTime.UtcNow).ToListAsync();
    }

    public async Task<int> GetActiveSessionsCountAsync(string userId)
    {
        return await CountAsync(x => x.UserId == userId && x.IsRevoked == false && x.ExpiresAt > DateTime.UtcNow);
    }

    public async Task<UserSession?> GetByDeviceIdAsync(string userId, string deviceId)
    {
        return await GetFirstOrDefaultAsync(x => x.UserId == userId && x.DeviceId == deviceId);
    }

    public async Task<UserSession?> GetByRefreshTokenAsync(string refreshToken)
    {
        return await GetFirstOrDefaultAsync(x => x.RefreshToken == refreshToken);
    }

    public async Task<UserSession?> GetLatestSessionByDeviceIdAsync(string userId, string deviceId)
    {
        return await _context.Set<UserSession>()
            .Where(x => x.UserId == userId && x.DeviceId == deviceId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();
    }
}

