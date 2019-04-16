using OrangeLoop.Sagas.Interfaces;
using System.Threading.Tasks;

namespace OrangeLoop.Sagas
{
    public delegate Task<T> UnitOfWorkStepMethod<T>(T context, IUnitOfWork unitOfWork);

    public class UnitOfWorkStep<T> : BaseSagaStep<UnitOfWorkStepMethod<T>>
    {
        public UnitOfWorkStep() : base() { }
        public UnitOfWorkStep(UnitOfWorkStepMethod<T> executeMethod, UnitOfWorkStepMethod<T> rollbackMethod) : base(executeMethod, rollbackMethod) { }

        public static UnitOfWorkStep<T> Create(UnitOfWorkStepMethod<T> executeMethod, UnitOfWorkStepMethod<T> rollbackMethod)
        {
            return new UnitOfWorkStep<T>(executeMethod, rollbackMethod);
        }

        public static UnitOfWorkStep<T> Create(UnitOfWorkStepMethod<T> executeMethod)
        {
            return new UnitOfWorkStep<T>(executeMethod, (ctx, uow) => Task.FromResult(ctx));
        }
    }
}
