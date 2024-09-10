using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OrangeLoop.Sagas.Interfaces;
using OrangeLoop.Sagas.UnitOfWork;
using OrangeLoop.Sagas.UnitOfWork.SqlServer;
using System.Data;

namespace OrangeLoop.Sagas.SqlServer.Extensions
{
    public static class SqlServerExtensions
    {
        public static IServiceCollection AddSqlServerUnitOfWork(this IServiceCollection services, string connectionStringName, IsolationLevel isolationLevel = IsolationLevel.ReadUncommitted) 
        {
            services.AddSingleton<IConnectionStringFactory>(provider 
                => new ConfigurationConnectionStringFactory(provider.GetRequiredService<IConfiguration>(), connectionStringName));

            return services.AddSqlServerUnitOfWork(isolationLevel);
        }

        public static IServiceCollection AddSqlServerUnitOfWork(this IServiceCollection services, string connectionStringName, string userId, string password, IsolationLevel isolationLevel = IsolationLevel.ReadUncommitted) 
        {
            services.AddSingleton<IConnectionStringFactory>(provider 
                => new ConfigurationConnectionStringBuilderFactory(provider.GetRequiredService<IConfiguration>(), connectionStringName, userId, password));

            return services.AddSqlServerUnitOfWork(isolationLevel);
        }

        #region Private Static Members
        private static IServiceCollection AddSqlServerUnitOfWork(this IServiceCollection services, IsolationLevel isolationLevel)
        {
            services.AddScoped<IUnitOfWork, DatabaseTransactionUnitOfWork>();
            return services.AddSqlServer(isolationLevel);
        }

        private static IServiceCollection AddSqlServer(this IServiceCollection services, IsolationLevel isolationLevel)
        {
            services.AddSingleton<IDatabaseTransactionConfiguration>(provider => new DatabaseTransactionConfiguration(isolationLevel));
            services.AddScoped<IConnectionFactory, SqlConnectionFactory>();
            return services;
        }
        #endregion
    }
}
