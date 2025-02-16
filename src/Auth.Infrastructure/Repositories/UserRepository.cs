
using System.Linq.Expressions;
using Auth.Application.Common.Interfaces;
using Auth.Domain.Entities;
using Auth.Infrastructure.Data;

namespace Auth.Infrastructure.Repositories;

public class UserRepository : GenericRepository<ApplicationUser>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context)
    {
    }

    public Task<ApplicationUser?> GetByIdAsync(string id, params Expression<Func<ApplicationUser, object>>[] includes)
    {
        throw new NotImplementedException();
    }

    public Task<ApplicationUser?> GetUserByEmailAsync(string email)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<UserSession>> GetUserSessionsAsync(string userId)
    {
        throw new NotImplementedException();
    }

    public Task<bool> IsEmailUniqueAsync(string email)
    {
        throw new NotImplementedException();
    }
}
