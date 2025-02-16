using Auth.Domain.Entities;

namespace Auth.Application.Common.Interfaces
{
    public interface IUserSessionRepository : IGenericRepository<UserSession>
    {
        Task<UserSession?> GetByRefreshTokenAsync(string refreshToken);
        Task<UserSession?> GetByDeviceIdAsync(string userId, string deviceId);
        Task<int> GetActiveSessionsCountAsync(string userId);
        Task<IEnumerable<UserSession>> GetActiveSessionsAsync(string userId);
    }
}
