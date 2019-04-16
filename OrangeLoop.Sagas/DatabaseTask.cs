using OrangeLoop.Sagas.Interfaces;
using System;
using System.Threading.Tasks;

namespace OrangeLoop.Sagas
{
    public class DatabaseTask : IUnitOfWorkTask
    {
        private readonly IUnitOfWorkFactory _factory;

        public DatabaseTask(IUnitOfWorkFactory factory)
        {
            _factory = factory;
        }

        public async Task ExecuteAsync(Func<IUnitOfWork, Task> func)
        {
            await SafeInvoke((uow) => func.Invoke(uow)).ConfigureAwait(false);
        }

        public async Task<T> ExecuteAsync<T>(Func<IUnitOfWork, Task<T>> func)
        {
            T result = default(T);
            await SafeInvoke(async (uow) => result = await func.Invoke(uow).ConfigureAwait(false)).ConfigureAwait(false);
            return result;
        }

        private async Task SafeInvoke(Func<IUnitOfWork, Task> func)
        {
            using (var unitOfWork = _factory.Create())
            {
                try
                {
                    await func.Invoke(unitOfWork).ConfigureAwait(false);
                    unitOfWork.Commit();
                }
                catch (Exception e)
                {
                    unitOfWork.Rollback();
                    throw e;
                }
            }
        }
    }
}
