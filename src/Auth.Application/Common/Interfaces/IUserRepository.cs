using Auth.Domain.Entities;
using System.Linq.Expressions;

namespace Auth.Application.Common.Interfaces
{
    public interface IUserRepository : IGenericRepository<ApplicationUser>
    {
        Task<ApplicationUser?> GetByIdAsync(string id, params Expression<Func<ApplicationUser, object>>[] includes);
        Task<ApplicationUser?> GetUserByEmailAsync(string email);
        Task<bool> IsEmailUniqueAsync(string email);
        Task<IEnumerable<UserSession>> GetUserSessionsAsync(string userId);
    }
}
