using Microsoft.Extensions.Configuration;
using OrangeLoop.Sagas.Interfaces;
using System.IO;

namespace OrangeLoop.Sagas
{
    public abstract class AppSettingsConnectionStringFactory : ConnectionStringReader, IConnectionStringFactory
    {
        public string Get()
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", true, true)
                .Build();

            return config.GetConnectionString(this.ConnectionName);
        }
    }
}
