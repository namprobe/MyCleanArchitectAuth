
using System.Linq.Expressions;
using Auth.Application.Common.Interfaces;
using Auth.Domain.Entities;
using Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

public class RoleRepository : GenericRepository<Role>, IRoleRepository
{
    public RoleRepository(ApplicationDbContext context) : base(context)
    {
    }

    // public Task<Role?> GetByIdAsync(string id, params Expression<Func<Role, object>>[] includes)
    // {
    //     throw new NotImplementedException();
    // }

    public async Task<Role?> GetRoleByNameAsync(string name)
    {
        return await GetFirstOrDefaultAsync(x => x.Name == name);
    }

    public async Task<IEnumerable<Role>> GetUserRolesAsync(string userId)
    {
        return await _context.UserRoles.Where(x => x.UserId == userId).Select(x => x.Role).ToListAsync();
    }
}

