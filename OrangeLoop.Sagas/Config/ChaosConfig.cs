using OrangeLoop.Sagas.Interfaces;
using System.Data;

namespace OrangeLoop.Sagas.Config
{
    public class ChaosConfig : IUnitOfWorkConfig
    {
        public IsolationLevel IsolationLevel => IsolationLevel.Chaos;
    }
}
