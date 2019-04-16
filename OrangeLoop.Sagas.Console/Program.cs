using Microsoft.Extensions.DependencyInjection;
using OrangeLoop.Sagas.Interfaces;
using System.Data.SqlClient;
using System.Threading.Tasks;
using System;
using Dapper;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;

namespace OrangeLoop.Sagas.Console
{
    class Program
    {
        public static async Task Main(string[] args)
        {
            System.Console.WriteLine(WindowsIdentity.GetCurrent().Name);
            var serviceProvider = new ServiceCollection()
                .AddTransient<IConnectionStringFactory, LiveFreeConnectionStringFactory>()
                .AddTransient<IUnitOfWorkConfig, DefaultConfig>()
                .AddSingleton<IUnitOfWorkFactory, DatabaseUnitOfWorkFactory<SqlConnection>>()
                .AddTransient<IUnitOfWorkTask, DatabaseTask>()
                .BuildServiceProvider();

            var task = serviceProvider.GetService<IUnitOfWorkTask>();

            var products = await task.ExecuteAsync<List<dynamic>>(async (uow) =>
            {
                var result = await uow.Transaction.Connection.QueryAsync<dynamic>("SELECT * FROM Products", null, uow.Transaction).ConfigureAwait(false);
                return result.ToList();

            }).ConfigureAwait(false);

            System.Console.WriteLine($"Found {products.Count()} Products");
            System.Console.ReadLine();
        }
    }
}
