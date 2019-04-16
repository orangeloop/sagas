using OrangeLoop.Sagas.Interfaces;
using System.Data;

namespace OrangeLoop.Sagas.Config
{
    public class SerializableConfig : IUnitOfWorkConfig
    {
        public IsolationLevel IsolationLevel => IsolationLevel.Serializable;
    }
}
