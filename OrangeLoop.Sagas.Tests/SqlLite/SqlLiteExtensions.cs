using Dapper;
using Microsoft.Extensions.DependencyInjection;
using OrangeLoop.Sagas.Interfaces;
using OrangeLoop.Sagas.Tests.SqlLite.Repositories;
using OrangeLoop.Sagas.UnitOfWork;
using System.Data;
using System.Threading.Tasks;

namespace OrangeLoop.Sagas.Tests.SqlLite
{
    public static class SqlLiteExtensions
    {
        public static IServiceCollection AddSqlLiteAsyncUnitOfWork(this IServiceCollection services)
        {
            services.AddSingleton<IConnectionStringFactory>(provider => new StaticConnectionStringFactory("Data Source=:memory:"));
            services.AddSingleton<IDatabaseTransactionConfiguration>(new DatabaseTransactionConfiguration(IsolationLevel.ReadCommitted));
            services.AddScoped<IConnectionFactory, SqliteConnectionFactory>();
            services.AddScoped<IUnitOfWork, DatabaseTransactionUnitOfWork>();
            services.AddScoped<ICustomersRepository, CustomersRepository>();
            return services;
        }

        public static async Task CreateTestSchema(this IDbConnection connection)
        {
            await connection.ExecuteAsync(@"
                CREATE TABLE Customers (
                    Id INTEGER,
                    FirstName TEXT,
                    LastName TEXT
                )");
        }
    }
}
