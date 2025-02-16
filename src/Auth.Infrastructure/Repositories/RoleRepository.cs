
using System.Linq.Expressions;
using Auth.Application.Common.Interfaces;
using Auth.Domain.Entities;
using Auth.Infrastructure.Data;

namespace Auth.Infrastructure.Repositories;

public class RoleRepository : GenericRepository<Role>, IRoleRepository
{
    public RoleRepository(ApplicationDbContext context) : base(context)
    {
    }

    public Task<Role?> GetByIdAsync(string id, params Expression<Func<Role, object>>[] includes)
    {
        throw new NotImplementedException();
    }

    public Task<Role?> GetRoleByNameAsync(string name)
    {
        throw new NotImplementedException();
    }

    public Task<IEnumerable<Role>> GetUserRolesAsync(string userId)
    {
        throw new NotImplementedException();
    }
}

