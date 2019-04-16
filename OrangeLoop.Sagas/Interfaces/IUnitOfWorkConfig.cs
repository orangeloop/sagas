using System.Data;

namespace OrangeLoop.Sagas.Interfaces
{
    public interface IUnitOfWorkConfig
    {
        IsolationLevel IsolationLevel { get; }
    }
}
