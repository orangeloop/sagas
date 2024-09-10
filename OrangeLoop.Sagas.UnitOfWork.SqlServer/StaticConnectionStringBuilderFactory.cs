using OrangeLoop.Sagas.Interfaces;
using System.Data.SqlClient;

namespace OrangeLoop.Sagas.UnitOfWork.SqlServer
{
    public class StaticConnectionStringBuilderFactory(string connectionString, string userId, string password)
        : StaticConnectionStringFactory(connectionString), IConnectionStringFactory
    {
        public override string Get() => new SqlConnectionStringBuilder(base.Get())
        {
            UserID = userId,
            Password = password
        }.ConnectionString;
    }
}
