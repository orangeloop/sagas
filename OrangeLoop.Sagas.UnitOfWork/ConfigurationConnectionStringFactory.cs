using Microsoft.Extensions.Configuration;
using OrangeLoop.Sagas.Interfaces;

namespace OrangeLoop.Sagas.UnitOfWork
{
    public class ConfigurationConnectionStringFactory(IConfiguration config, string keyName) : IConnectionStringFactory
    {
        public virtual string Get() => config.GetConnectionString(keyName) ?? string.Empty;
    }
}
