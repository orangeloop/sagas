using OrangeLoop.Sagas.Interfaces;
using System.Collections.Generic;

namespace OrangeLoop.Sagas
{
    public class SagaConfiguration<T> : ISagaConfiguration<StepMethod<T>>
    {
        public LinkedList<ISagaStep<StepMethod<T>>> Steps { get; private set; }

        public SagaConfiguration()
        {
            Steps = new LinkedList<ISagaStep<StepMethod<T>>>();
        }

        public ISagaStep<StepMethod<T>> AddStep(ISagaStep<StepMethod<T>> step)
        {
            Steps.AddLast(step);
            return step;
        }

        public ISagaStep<StepMethod<T>> AddStep(StepMethod<T> step)
        {
            return this.AddStep(SagaStep<T>.Create(step));
        }
    }
}
