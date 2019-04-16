using OrangeLoop.Sagas.Interfaces;
using System.Data;

namespace OrangeLoop.Sagas.Config
{
    public class ReadUncommittedConfig : IUnitOfWorkConfig
    {
        public IsolationLevel IsolationLevel => IsolationLevel.ReadUncommitted;
    }
}
