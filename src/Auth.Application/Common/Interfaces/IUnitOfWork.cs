namespace Auth.Application.Common.Interfaces 
{
    public interface IUnitOfWork : IDisposable
    {
        IUserRepository Users { get; }
        IRoleRepository Roles { get; }
        IUserSessionRepository UserSessions { get; }
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
        Task<TResult> ExecuteTransactionAsync<TResult>(Func<Task<TResult>> action);
    }
}
