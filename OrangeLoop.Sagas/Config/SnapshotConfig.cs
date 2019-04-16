using OrangeLoop.Sagas.Interfaces;
using System.Data;

namespace OrangeLoop.Sagas.Config
{
    public class SnapshotConfig : IUnitOfWorkConfig
    {
        public IsolationLevel IsolationLevel => IsolationLevel.Snapshot;
    }
}
