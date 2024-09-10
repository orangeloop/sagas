using OrangeLoop.Sagas.Interfaces;

namespace OrangeLoop.Sagas.UnitOfWork
{
    public class StaticConnectionStringFactory(string connectionString) : IConnectionStringFactory
    {
        public virtual string Get() => connectionString;
    }
}
