
using Auth.Application.Common.Interfaces;
using Auth.Domain.Entities;
using Auth.Infrastructure.Data;

namespace Auth.Infrastructure.Repositories;

public class UserSessionRepository : GenericRepository<UserSession>, IUserSessionRepository
{
    public UserSessionRepository(ApplicationDbContext context) : base(context)
    {
        
    }

    public Task<IEnumerable<UserSession>> GetActiveSessionsAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<int> GetActiveSessionsCountAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<UserSession?> GetByDeviceIdAsync(string userId, string deviceId)
    {
        throw new NotImplementedException();
    }

    public Task<UserSession?> GetByRefreshTokenAsync(string refreshToken)
    {
        throw new NotImplementedException();
    }
}

