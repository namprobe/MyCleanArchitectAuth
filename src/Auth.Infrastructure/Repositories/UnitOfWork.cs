
using System.Security;
using Auth.Application.Common.Interfaces;
using Auth.Infrastructure.Data;

namespace Auth.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;
    private IUserRepository _users;
    private IRoleRepository _roles;
    private IUserSessionRepository _userSessions;

    public UnitOfWork(ApplicationDbContext context, IUserRepository users, IRoleRepository roles, IUserSessionRepository userSessions)
    {
        _context = context;
        _users = users;
        _roles = roles;
        _userSessions = userSessions;
    }

    public IUserRepository Users => _users;

    public IRoleRepository Roles => _roles;

    public IUserSessionRepository UserSessions => _userSessions;

    public async Task BeginTransactionAsync()
    {
        await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
        try
        {
            await _context.Database.CommitTransactionAsync();
        }
        catch 
        {
            await _context.Database.RollbackTransactionAsync();
            throw;
        }
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    public async Task<TResult> ExecuteTransactionAsync<TResult>(Func<Task<TResult>> action)
    {
        await BeginTransactionAsync();
        try
        {
            var result = await action();
            await CommitAsync();
            return result;
        }
        catch 
        {
            await RollbackAsync();
            throw;
        }
    }

    public async Task ExecuteTransactionAsync(Func<Task> action)
    {
        await BeginTransactionAsync();
        try
        {
            await action();
            await CommitAsync();
        }
        catch
        {
            await RollbackAsync();
            throw;
        }
    }

    public async Task RollbackAsync()
    {
        await _context.Database.RollbackTransactionAsync();
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}

