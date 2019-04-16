using OrangeLoop.Sagas.Interfaces;
using System.Data;

namespace OrangeLoop.Sagas
{
    public class DefaultConfig : IUnitOfWorkConfig
    {
        public IsolationLevel IsolationLevel => IsolationLevel.ReadUncommitted;
    }
}
