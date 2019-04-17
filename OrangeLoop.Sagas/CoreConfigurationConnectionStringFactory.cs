using Microsoft.Extensions.Configuration;
using OrangeLoop.Sagas.Interfaces;

namespace OrangeLoop.Sagas
{
    public abstract class CoreConfigurationConnectionStringFactory : ConnectionStringReader, IConnectionStringFactory
    {
        private readonly IConfiguration _config;

        public CoreConfigurationConnectionStringFactory(IConfiguration config)
        {
            _config = config;
        }

        public string Get()
        {
            return _config.GetValue<string>(ConnectionName);
        }
    }
}
