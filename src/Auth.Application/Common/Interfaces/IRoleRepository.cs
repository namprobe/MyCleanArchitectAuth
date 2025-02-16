using Auth.Domain.Entities;
using System.Linq.Expressions;

namespace Auth.Application.Common.Interfaces
{
    public interface IRoleRepository : IGenericRepository<Role>
    {
        Task<Role?> GetByIdAsync(string id, params Expression<Func<Role, object>>[] includes);
        
        Task<Role?> GetRoleByNameAsync(string name);
        Task<IEnumerable<Role>> GetUserRolesAsync(string userId);
    }
    
}
