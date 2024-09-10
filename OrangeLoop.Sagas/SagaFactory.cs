using OrangeLoop.Sagas.Interfaces;

namespace OrangeLoop.Sagas
{
    public class SagaFactory : ISagaFactory
    {
        public ISaga<T> Create<T>() where T : class
            => new Saga<T>();
    }
}
