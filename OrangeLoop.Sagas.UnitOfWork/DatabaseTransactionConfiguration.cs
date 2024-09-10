using OrangeLoop.Sagas.Interfaces;
using System.Data;

namespace OrangeLoop.Sagas.UnitOfWork
{
    public class DatabaseTransactionConfiguration(IsolationLevel isolationLevel) : IDatabaseTransactionConfiguration
    {
        public DatabaseTransactionConfiguration() : this(IsolationLevel.ReadUncommitted) { }

        public IsolationLevel IsolationLevel => isolationLevel;
    }
}
