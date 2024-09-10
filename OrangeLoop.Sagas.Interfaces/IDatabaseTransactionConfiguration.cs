using System.Data;

namespace OrangeLoop.Sagas.Interfaces
{
    public interface IDatabaseTransactionConfiguration
    {
        public IsolationLevel IsolationLevel { get; }
    }
}
