using System.Data;

namespace OrangeLoop.Sagas.Interfaces
{
    public interface IConnectionFactory
    {
        IDbConnection Get();
        Task<IDbConnection> GetAsync();
    }
}
