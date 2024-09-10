using OrangeLoop.Sagas.Interfaces.Models;

namespace OrangeLoop.Sagas.Interfaces
{
    public interface ISaga<T> where T : class
    {
        ISaga<T> AddStep(Action<ISagaStep<T>> action);
        Task<ExecutionResult<T>> Run(T context);
    }
}
