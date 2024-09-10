using OrangeLoop.Sagas.Interfaces;
using OrangeLoop.Sagas.Interfaces.Models;
using System.Threading.Tasks;

namespace OrangeLoop.Sagas
{
    public class UnitOfWorkSaga<T>(IUnitOfWork unitOfWork) : Saga<T> where T : class
    {
        public override Task<ExecutionResult<T>> Run(T context)
            => unitOfWork.ExecuteAsync<T>(async () =>
            {
                var result = await base.Run(context);
                return result.Success
                    ? result.Value
                    : throw result.Error;
            });
    }
}
