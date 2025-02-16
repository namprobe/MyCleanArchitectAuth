
using Auth.Application.Common.Interfaces;
using Auth.Infrastructure.Data;

namespace Auth.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    public IUserRepository Users => throw new NotImplementedException();

    public IRoleRepository Roles => throw new NotImplementedException();

    public IUserSessionRepository UserSessions => throw new NotImplementedException();

    public Task BeginTransactionAsync()
    {
        throw new NotImplementedException();
    }

    public Task CommitAsync()
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }

    public Task<TResult> ExecuteTransactionAsync<TResult>(Func<Task<TResult>> action)
    {
        throw new NotImplementedException();
    }

    public Task ExecuteTransactionAsync(Func<Task> action)
    {
        throw new NotImplementedException();
    }

    public Task RollbackAsync()
    {
        throw new NotImplementedException();
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}

