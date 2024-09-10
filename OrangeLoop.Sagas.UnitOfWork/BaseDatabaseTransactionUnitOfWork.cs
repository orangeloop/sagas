using OrangeLoop.Sagas.Interfaces;
using OrangeLoop.Sagas.Interfaces.Models;
using System.Data;

namespace OrangeLoop.Sagas.UnitOfWork
{
    public abstract class BaseDatabaseTransactionUnitOfWork(IConnectionFactory connectionFactory) : IUnitOfWork
    {
        public abstract IDbTransaction? CurrentTransaction { get; protected set; }
        protected IDbConnection Connection => connectionFactory.Get();

        public abstract Task RollbackAsync();
        public abstract Task StartTransactionAsync();
        public abstract Task CommitAsync();

        public virtual async Task<ExecutionResult<T>> ExecuteAsync<T>(Func<Task<T>> func) where T : class
        {
            try
            {
                await StartTransactionAsync();
                T result = await func();
                await CommitAsync();
                return ExecutionResult<T>.CreateSuccess(result);
            }
            catch(Exception ex)
            {
                await RollbackAsync();
                return ExecutionResult<T>
                    .CreateFailure()
                    .AppendException(ex); 
            }
        }
        public virtual async Task<ExecutionResult<T>> ExecuteAsync<T>(Func<Func<T, Task>, Func<T, Exception, Task>, Task> func) where T : class
        {
            try
            {
                ExecutionResult<T> result = ExecutionResult<T>.CreateEmpty();
                await StartTransactionAsync();
                await func(async context =>
                {
                    await CommitAsync();
                    result.SetSuccess(context);
                },
                async (context, error) =>
                {
                    await RollbackAsync();
                    result = result
                        .SetFailure(context)
                        .AppendException(error); 
                });

                return result;
            }
            catch(Exception ex)
            {
                await RollbackAsync();
                return ExecutionResult<T>
                    .CreateFailure()
                    .AppendException(ex); 
            }
        }

        public virtual async Task<ExecutionResult> ExecuteAsync(Func<Task> func)
        {
            try
            {
                await StartTransactionAsync();
                await func();
                await CommitAsync();
                return ExecutionResult.CreateSuccess();
            }
            catch(Exception ex)
            {
                await RollbackAsync();
                return ExecutionResult
                    .CreateFailure()
                    .AppendException(ex);
            }
        }

        public virtual async Task<ExecutionResult> ExecuteAsync(Func<Func<Task>, Func<Exception, Task>, Task> func)
        {
            try
            {
                ExecutionResult result = ExecutionResult.CreateEmpty();
                await StartTransactionAsync();
                await func(async () =>
                {

                    await CommitAsync();
                    result.SetSuccess();
                }, async error =>
                {
                    await RollbackAsync();
                    result
                        .SetFailure()
                        .AppendException(error);
                });
                return result;
            }
            catch(Exception ex)
            {
                await RollbackAsync();
                return ExecutionResult
                    .CreateFailure()
                    .AppendException(ex);
            }
        }
    }
}
