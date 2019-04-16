using OrangeLoop.Sagas.Interfaces;
using System.Data;

namespace OrangeLoop.Sagas.Config
{
    public class ReadCommittedConfig : IUnitOfWorkConfig
    {
        public IsolationLevel IsolationLevel => IsolationLevel.ReadCommitted;
    }
}
