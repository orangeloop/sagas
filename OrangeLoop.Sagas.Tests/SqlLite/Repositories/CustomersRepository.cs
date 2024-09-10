using Dapper;
using Microsoft.Extensions.DependencyInjection;
using OrangeLoop.Sagas.Interfaces;
using OrangeLoop.Sagas.Tests.SqlLite.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrangeLoop.Sagas.Tests.SqlLite.Repositories
{
    public interface ICustomersRepository
    {
        Task Create(Customer customer, bool throwException = false);
        Task Update(Customer customer, bool throwException = false);
        Task Delete(int id, bool throwException = false);
        Task<Customer> Get(int id, bool throwException = false);

    }
    public class CustomersRepository(IConnectionFactory connectionFactory, IUnitOfWork unitOfWork) : ICustomersRepository
    {
        public async Task Create(Customer customer, bool throwException = false)
        {
            if (throwException) throw new Exception();

            var conn = await connectionFactory.GetAsync();
            await conn.ExecuteAsync(@"
                INSERT INTO Customers (
                    Id,
                    FirstName,
                    LastName
                ) VALUES (
                    @Id,
                    @FirstName,
                    @LastName
                )
            ", customer, transaction: unitOfWork.CurrentTransaction);
        }

        public async Task Delete(int id, bool throwException = false)
        {
            if (throwException) throw new Exception();

            var conn = await connectionFactory.GetAsync();
            await conn.ExecuteAsync(@"DELETE FROM Customers where Id = @Id", new { Id = id }, transaction: unitOfWork.CurrentTransaction);
        }

        public async Task<Customer> Get(int id, bool throwException = false)
        {
            if (throwException) throw new Exception();

            var conn = await connectionFactory.GetAsync();
            return await conn.QueryFirstOrDefaultAsync<Customer>(@"
                SELECT * FROM Customers WHERE Id = @Id", new { Id = id }, transaction: unitOfWork.CurrentTransaction);
        }

        public async Task Update(Customer customer, bool throwException = false)
        {
            if (throwException) throw new Exception();

            var conn = await connectionFactory.GetAsync();
            await conn.ExecuteAsync(@"
                UPDATE Customers
                    SET
                        FirstName = @FirstName,
                        LastName = @LastName
                    WHERE Id = @Id", customer, transaction: unitOfWork.CurrentTransaction);
        }
    }
}
