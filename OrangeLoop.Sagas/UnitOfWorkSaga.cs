using OrangeLoop.Sagas.Interfaces;
using System;
using System.Threading.Tasks;

namespace OrangeLoop.Sagas
{
    public abstract class UnitOfWorkSaga<T> : BaseSaga<UnitOfWorkStepMethod<T>, T>
    {
        private readonly IUnitOfWorkFactory _unitOfWorkFactory;

        public UnitOfWorkSaga(IUnitOfWorkFactory unitOfWorkFactory) : base(new UnitOfWorkSagaConfiguration<T>())
        {
            _unitOfWorkFactory = unitOfWorkFactory;
        }

        public override async Task<T> Run(T context)
        {
            using (var unitOfWork = _unitOfWorkFactory.Create())
            {
                try
                {
                    context = await base.Run(context, (func, ctx) => {
                        return func.Invoke(ctx, unitOfWork);
                    });

                    unitOfWork.Commit();

                    return context;
                }
                catch (Exception)
                {
                    unitOfWork.Rollback();
                    throw;
                }
            }
        }
    }
}
