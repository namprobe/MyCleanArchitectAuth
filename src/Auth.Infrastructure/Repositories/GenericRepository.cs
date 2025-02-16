using System.Linq.Expressions;
using Auth.Application.Common.Interfaces;
using Auth.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Auth.Infrastructure.Repositories;

public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
{
    protected readonly ApplicationDbContext _context;
    private readonly DbSet<TEntity> _dbSet;

    public GenericRepository(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        _dbSet = _context.Set<TEntity>();
    }

    // Add GetEntityId helper method
    protected virtual object GetEntityId(TEntity entity)
    {
        var keyProperty = _context.Model.FindEntityType(typeof(TEntity))
            ?.FindPrimaryKey()?.Properties[0];

        if (keyProperty == null)
            throw new InvalidOperationException("Entity does not have a primary key");

        return keyProperty.GetGetter().GetClrValue(entity);
    }

    public virtual async Task<TEntity> AddAsync(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var entry = await _dbSet.AddAsync(entity);
        return entry.Entity;
    }

    public virtual async Task DeleteAsync(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var entityState = _context.Entry(entity).State;
        if (entityState == EntityState.Detached)
        {
            var id = GetEntityId(entity);
            var trackedEntity = await _dbSet.FindAsync(id);
            if (trackedEntity == null)
                throw new KeyNotFoundException($"Entity with id {id} not found");

            _dbSet.Remove(trackedEntity);
        }
        else
        {
            _dbSet.Remove(entity);
        }
    }

    public virtual IQueryable<TEntity> FindByCondition(Expression<Func<TEntity, bool>> expression)
    {
        return _dbSet.AsNoTracking().Where(expression);
    }

    public virtual async Task<IEnumerable<TEntity>> GetAllAsync()
    {
        return await _dbSet.AsNoTracking().ToListAsync();
    }

    public virtual async Task<TEntity> GetByIdAsync(Guid id, params Expression<Func<TEntity, object>>[] includes)
    {
        // If no includes, use simple query
        if (includes == null || !includes.Any())
        {
            return await _dbSet.FirstOrDefaultAsync(e => EF.Property<Guid>(e, "Id") == id);
        }

        // If has includes, use GetFirstOrDefaultAsync
        return await GetFirstOrDefaultAsync(
            e => EF.Property<Guid>(e, "Id") == id,
            includes);
    }

    public virtual async Task<TEntity> GetFirstOrDefaultAsync(
        Expression<Func<TEntity, bool>> predicate,
        params Expression<Func<TEntity, object>>[] includes)
    {
        var query = _dbSet.AsQueryable();

        if (includes?.Any() == true)
        {
            query = includes.Aggregate(query,
                (current, include) => current.Include(include));
        }
        return await query.FirstOrDefaultAsync(predicate);
    }

    public virtual async Task<TEntity> UpdateAsync(TEntity entity)
    {
        if (entity == null)
            throw new ArgumentNullException(nameof(entity));

        var id = GetEntityId(entity);
        var existingEntity = await _dbSet.FindAsync(id);

        if (existingEntity == null)
            throw new KeyNotFoundException($"Entity with id {id} not found");

        _context.Entry(existingEntity).CurrentValues.SetValues(entity);

        foreach (var navigation in _context.Entry(existingEntity).Navigations)
        {
            if (navigation.Metadata.IsCollection)
                continue;

            var navigationValue = navigation.Metadata.PropertyInfo?.GetValue(entity);
            if (navigationValue != null)
            {
                navigation.CurrentValue = navigationValue;
            }
        }

        _context.Entry(existingEntity).State = EntityState.Modified;
        return existingEntity;
    }

    public async Task<int> CountAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return await _dbSet.CountAsync(predicate);
    }
}

