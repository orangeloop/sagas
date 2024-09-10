using OrangeLoop.Sagas.Interfaces.Models;
using System.Data;

namespace OrangeLoop.Sagas.Interfaces
{
    public interface IUnitOfWork
    {
        IDbTransaction? CurrentTransaction { get; }

        Task StartTransactionAsync();

        Task CommitAsync();

        Task RollbackAsync();

        Task<ExecutionResult<T>> ExecuteAsync<T>(Func<Task<T>> func) where T : class;
        Task<ExecutionResult<T>> ExecuteAsync<T>(Func<Func<T, Task>, Func<T, Exception, Task>, Task> func) where T : class;

        Task<ExecutionResult> ExecuteAsync(Func<Task> func);
        Task<ExecutionResult> ExecuteAsync(Func<Func<Task>, Func<Exception, Task>, Task> func);
    }
}
