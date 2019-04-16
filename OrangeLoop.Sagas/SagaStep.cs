using System.Threading.Tasks;

namespace OrangeLoop.Sagas
{
    public delegate Task<T> StepMethod<T>(T context);

    public class SagaStep<T> : BaseSagaStep<StepMethod<T>>
    {
        public SagaStep() : base() { }
        public SagaStep(StepMethod<T> executeMethod, StepMethod<T> rollbackMethod) : base(executeMethod, rollbackMethod) { }

        public static SagaStep<T> Create(StepMethod<T> executeMethod, StepMethod<T> rollbackMethod)
        {
            return new SagaStep<T>(executeMethod, rollbackMethod);
        }

        public static SagaStep<T> Create(StepMethod<T> executeMethod)
        {
            return new SagaStep<T>(executeMethod, (ctx) => Task.FromResult(ctx));
        }
    }
}
