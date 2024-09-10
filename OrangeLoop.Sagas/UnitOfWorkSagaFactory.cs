using OrangeLoop.Sagas.Interfaces;

namespace OrangeLoop.Sagas
{
    public class UnitOfWorkSagaFactory(IUnitOfWork unitOfWork) : IUnitOfWorkSagaFactory
    {
        public ISaga<T> Create<T>() where T : class
            => new UnitOfWorkSaga<T>(unitOfWork);
    }
}
