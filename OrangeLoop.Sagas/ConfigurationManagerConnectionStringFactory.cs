using OrangeLoop.Sagas.Interfaces;
using System.Configuration;

namespace OrangeLoop.Sagas
{
    public abstract class ConfigurationManagerConnectionStringFactory : ConnectionStringReader, IConnectionStringFactory
    {
        public string Get()
        {
            return ConfigurationManager.ConnectionStrings[this.ConnectionName].ConnectionString;
        }
    }
}
