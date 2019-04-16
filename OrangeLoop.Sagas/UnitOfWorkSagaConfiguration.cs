using OrangeLoop.Sagas.Interfaces;
using System.Collections.Generic;

namespace OrangeLoop.Sagas
{
    public class UnitOfWorkSagaConfiguration<T> : ISagaConfiguration<UnitOfWorkStepMethod<T>>
    {
        public LinkedList<ISagaStep<UnitOfWorkStepMethod<T>>> Steps { get; private set; }

        public UnitOfWorkSagaConfiguration()
        {
            Steps = new LinkedList<ISagaStep<UnitOfWorkStepMethod<T>>>();
        }

        public ISagaStep<UnitOfWorkStepMethod<T>> AddStep(ISagaStep<UnitOfWorkStepMethod<T>> step)
        {
            Steps.AddLast(step);
            return step;
        }

        public ISagaStep<UnitOfWorkStepMethod<T>> AddStep(UnitOfWorkStepMethod<T> step)
        {
            return this.AddStep(UnitOfWorkStep<T>.Create(step));
        }
    }
}
