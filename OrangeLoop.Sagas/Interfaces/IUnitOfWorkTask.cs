using System;
using System.Threading.Tasks;

namespace OrangeLoop.Sagas.Interfaces
{
    public interface IUnitOfWorkTask
    {
        Task ExecuteAsync(Func<IUnitOfWork, Task> func);
        Task<T> ExecuteAsync<T>(Func<IUnitOfWork, Task<T>> func);
    }
}
