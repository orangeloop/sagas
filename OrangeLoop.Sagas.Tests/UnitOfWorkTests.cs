using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OrangeLoop.Sagas.Interfaces;
using OrangeLoop.Sagas.Tests.SqlLite;
using System;
using System.Threading.Tasks;
using Dapper;
using System.Data;
using OrangeLoop.Sagas.Tests.SqlLite.Models;
using OrangeLoop.Sagas.Tests.SqlLite.Repositories;

namespace OrangeLoop.Sagas.Tests
{
    [TestClass]
    public class UnitOfWorkTests
    {
        private readonly IServiceCollection services = new ServiceCollection();
        private IServiceProvider provider;
        private IServiceScope scope;
        private IUnitOfWork unitOfWork;
        private ICustomersRepository customersRepository;

        [TestInitialize]
        public async Task Init()
        {
            services.AddSqlLiteAsyncUnitOfWork();
            provider = services.BuildServiceProvider();
            scope = provider.CreateScope();
            unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            customersRepository = scope.ServiceProvider.GetRequiredService<ICustomersRepository>();
            await Connection.CreateTestSchema();
        }

        [TestCleanup]
        public void Cleanup()
        {
            scope.Dispose();
        }

        [TestMethod]
        public async Task UnitOfWork_PersistsChanges_WhenNoErrors()
        {
            // Act
            var result = await unitOfWork.ExecuteAsync(async () =>
            {
                await customersRepository.Create(TestData.Customers[0]);
                await customersRepository.Create(TestData.Customers[1]);
            });

            // Assert
            Assert.IsTrue(result.Success);
            var count = await RecordCount();
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public async Task UnitOfWork_WithModel_PersistsChanges_WhenNoErrors()
        {
            // Act
            var result = await unitOfWork.ExecuteAsync(async () =>
            {
                await customersRepository.Create(TestData.Customers[0]);
                await customersRepository.Create(TestData.Customers[1]);
                return TestData.Customers;
            });

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual(2, result.Value.Count);
            var count = await RecordCount();
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public async Task UnitOfWork_WithCallbacksAndModel_PersistsChanges_WhenNoErrors()
        {
            // Act
            var result = await unitOfWork.ExecuteAsync<Customer>(async (success, failure) =>
            {
                await customersRepository.Create(TestData.Customers[0]);
                await customersRepository.Create(TestData.Customers[1]);
                await success(TestData.Customers[0]);
            });

            // Assert
            Assert.IsTrue(result.Success);
            Assert.IsInstanceOfType<Customer>(result.Value);
            var count = await RecordCount();
            Assert.AreEqual(2, count);
        }

        [TestMethod]
        public async Task UnitOfWork_RevertsChanges_WhenErrors()
        {
            var result = await unitOfWork.ExecuteAsync(async () =>
                {
                    await customersRepository.Create(TestData.Customers[0]);
                    await customersRepository.Create(TestData.Customers[1], throwException: true);
                });

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            var count = await RecordCount();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public async Task UnitOfWork_WithCallbacksAndModel_RevertsChanges_WhenErrors()
        {
            var result = await unitOfWork.ExecuteAsync<Customer>(async (success, failure) =>
                {
                    try
                    {
                        await customersRepository.Create(TestData.Customers[0]);
                        await customersRepository.Create(TestData.Customers[1], throwException: true);
                        await success(TestData.Customers[0]);
                    }
                    catch (Exception ex)
                    {
                        await failure(TestData.Customers[0], ex);
                    }
                });

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Assert.IsNotNull(result.Value);
            Assert.IsInstanceOfType<AggregateException>(result.Error);
            var count = await RecordCount();
            Assert.AreEqual(0, count);
        }

        [TestMethod]
        public async Task UnitOfWork_WithCallbacks_RevertsChanges_WhenErrors()
        {
            var result = await unitOfWork.ExecuteAsync(async (success, failure) =>
                {
                    try
                    {
                        await customersRepository.Create(TestData.Customers[0]);
                        await customersRepository.Create(TestData.Customers[1], throwException: true);
                        await success();
                    }
                    catch (Exception ex)
                    {
                        await failure(ex);
                    }
                });

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsNotNull(result.Error);
            Assert.IsInstanceOfType<AggregateException>(result.Error);
            var count = await RecordCount();
            Assert.AreEqual(0, count);
        }

        private Task<int> RecordCount()
            => Connection.ExecuteScalarAsync<int>("SELECT COUNT(*) FROM Customers");

        private IDbConnection Connection => scope.ServiceProvider.GetRequiredService<IConnectionFactory>().Get();
    }
}
