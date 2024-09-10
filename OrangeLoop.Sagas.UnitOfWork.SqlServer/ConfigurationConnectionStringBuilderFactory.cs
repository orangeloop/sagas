using Microsoft.Extensions.Configuration;
using OrangeLoop.Sagas.Interfaces;
using System.Data.SqlClient;

namespace OrangeLoop.Sagas.UnitOfWork.SqlServer
{
    public class ConfigurationConnectionStringBuilderFactory(IConfiguration config, string keyName, string userId, string password)
        : ConfigurationConnectionStringFactory(config, keyName), IConnectionStringFactory
    {
        public override string Get()
            => new SqlConnectionStringBuilder(base.Get())
            {
                UserID = userId,
                Password = password
            }.ConnectionString;
    }
}
