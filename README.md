# Unit Of Work and Sagas
[![Build status](https://ci.appveyor.com/api/projects/status/000g0ngc7jkf58sx?svg=true)](https://ci.appveyor.com/project/orangeloop/sagas)

## Installation

##### .Net Core CLI
`dotnet add package OrangeLoop.Sagas`

##### Package Manager Console

`Install-Package OrangeLoop.Sagas`

## Overview

Often, our business processes are spread across multiple services and repositories.  When a customer completes an order, for example,
we need to update the status of the payment and the order in the database.  This simple scenario could
involve our `PaymentsRepository` and our `OrdersRepository`, and must fail or succeed together.  If an exception occurs while updating
the order status in the database, we would need to rollback any changes we made to the payment status as well.

In this example, we assume our business process executes in the same application domain (e.g. Web Request), and that the operations which must all 
succeed or fail together involve the database.  The traditional way to handle this is to wrap our queries in an `IDbTransaction`, and 
rollback the transaction if there's an exception.  The challenge with using the `IDbTransaction` directly is that our Order related
query and our Payment related query must be grouped together.  This doesn't work when using the Repository Pattern, and injecting repositories
into our services.

The **Unit of Work** pattern creates an abstraction of an IDbTransaction, and allows us to define a "Unit of Work" in our Services, rather than
in our Repositories.

What about cross-domain business processes?  If we have an `Orders` microservice, and a `Payments` microservice, we must
make a call to each in order to update the `Order` and `Payment` statuses. The queries for each service execute 
in separate processes, and using an `IDbTransaction` is simply not an option. **Sagas** to the rescue.  A **Saga** defines a set of steps that
must all succeed or fail together, and allows us to explicitly define the rollback procedure for each step.

Let's take a closer look at each of these scenarios, starting with a Unit of Work.

## Unit Of Work

##### `IUnitOfWork.cs`

```CSharp
    public interface IUnitOfWork : IDisposable
    {
        IDbTransaction Transaction { get; }
        void Commit();
        void Rollback();
    }

```

Given an instance of `IUnitOfWork`, our usage might look like:

```CSharp
    using(IUnitOfWork unitOfWork = GetInstance())
    {
        try
        {
            // Database operations
            // We can provide our database operations with an IDbTransaction
            // using the unitOfWork.Transaction property.
            
            unitOfWork.Commit();
        }
        catch
        {
            unitOfWork.Rollback();
        }
    }
```

Normally, we delegate the creation of an `IUnitOfWork` instance to an `IUnitOfWorkFactory`.  The `DatabaseUnitOfWorkFactory<T, K>` is provided
for this purpose, where `T` is an `IDbConnection` and `K` is an `IUnitOfWorkConfig`.  The `DatabaseUnitOfWorkFactory` constructor takes an `IConnectionStringFactory`. 

An `IConnectionStringFactory` is responsible for providing the database connection string, and the
`IUnitOfWorkConfig` provides the transaction **IsolationLevel**. The following classes are provided and implement the `IUnitOfWorkConfig` interface: `ChaosConfig`, 
`ReadCommittedConfig`, `ReadUncommittedConfig`, `RepeatableReadConfig`, `SerializableConfig`, `SnapshotConfig`.

Since no two applications store a connection string the same way, a default implementation of `IConnectionStringFactory` is not provided. It is, however,
common to store a connection string in a configuration file's **ConnectionStrings** section. Two abstract base classes are provided for
reading from  _appsettings.json_ or _*.config_: `AppSettingsConnectionStringFactory` and `ConfigurationManagerConnectionStringFactory`. You can inherit
from one of these classes and override the *ConnectionName* property.  For example, given the following _web.config_ file:

```xml
    <configuration>
        <connectionStrings>
            <add name="ExampleDB" connectionString="..." providerName="System.Data.SqlClient" />
        </connectionStrings>
    </configuration>
```

We would create the `ExampleDBConnectionStringFactory` class:

```CSharp
    public class ExampleDBConnectionStringFactory : ConfigurationManagerConnectionStringFactory
    {
        protected override string ConnectionName => "ExampleDB";
    }
```

##### DatabaseUnitOfWorkFactory

We now have everything we need to create a `DatabaseUnitOfWorkFactory`.

```CSharp
    var factory = new DatabaseUnitOfWorkFactory<SqlConnection, ReadCommittedConfig>(
        new ExampleDBConnectionStringFactory()
    );

    using(IUnitOfWork unitOfWork = factory.Create())
    {
        try
        {
            // Same as example above
            unitOfWork.Commit();
        }
        catch
        {
            unitOfWork.Rollback();
        }
    }
```

Notice that in the example, we repeat the same try/catch pattern as before.  This isn't very DRY, and pretty soon our code would 
be littered with try/catch blocks. We can use the `DatabaseTask` class to avoid repeating ourselves.

```CSharp
    var factory = new DatabaseUnitOfWorkFactory<SqlConnection, ReadCommittedConfig>(
        new ExampleDBConnectionStringFactory()
    );

    var task = new DatabaseTask(factory);
    await task.ExecuteAsync(async (unitOfWork) =>
    {
       // Do database stuff
 
    }).ConfigureAwait(false);
``` 

#### Dependency Injection

Normally you'll want to plug all this into your DI framework so you can simply inject an `IDatabaseTask` or an `IUnitOfWorkFactory` into your services.

```CSharp
    var services = new ServiceCollection()
        .AddSingleton<IUnitOfWorkFactory, DatabaseUnitOfWorkFactory<SqlConnection, ReadCommittedConfig>()
        .AddSingleton<IConnectionStringFactory, ExampleDBConnectionStringFactory>()
        .AddSingleton<IUnitOfWorkTask, DatabaseTask>()
        .Build();
```

#### Example

Let's look at an example of how we could implement the scenario of updating both the payment status and the order status. (This
example is using [Dapper](https://github.com/StackExchange/Dapper))

```CSharp
    // IOrdersRepository.cs
    public interface IOrdersRepository
    {
        Task<long> GetPayment(IUnitOfWork unitOfWork, long orderId);
        Task UpdateStatus(IUnitOfWork unitOfWork, long orderId, string status);
    }

    // IPaymentsRepository.cs
    public interface IPaymentsRepository
    {
        Task UpdateStatus(IUnitOfWork unitOfWork, long paymentId, string status);
    }

    // OrdersRepository.cs
    public class OrdersRepository : IOrdersRepository
    {
        public async Task<long> GetPayment(IUnitOfWork unitOfWork, long orderId)
        {
            var conn = unitOfWork.Transaction.Connection;
            var id = await conn.QueryAsync<long>("SELECT ...", new { OrderID = orderId }, unitOfWork.Transaction).ConfigureAwait(false);
            return id;
        } 

        public async Task UpdateStatus(IUnitOfWork unitOfWork, long orderId, string status)
        {
            var conn = unitOfWork.Transaction.Connection;
            await conn.ExecuteAsync("UPDATE ...", new { OrderID = orderId, Status = status }, unitOfWork.Transaction).ConfigureAwait(false);
        }
    }

    // PaymentsRepository.cs
    public class PaymentsRepository : IPaymentsRepository
    {
        public async Task UpdateStatus(IUnitOfWork unitOfWork, long paymentId, string status)
        {
            var conn = unitOfWork.Transaction.Connection;
            await conn.ExecuteAsync("UPDATE ...", new { PaymentID = paymentId, Status = status }, unitOfWork.Transaction).ConfigureAwait(false);
        }
    }

    // IOrderPaymentService.cs
    public interface IOrderPaymentService
    {
        Task CompleteSuccess(long orderId);
    }

    // OrderPaymentService.cs
    public class OrderPaymentService : IOrderPaymentService
    {
        private readonly IDatabaseTask _databaseTask;
        private readonly IOrdersRepository _ordersRepository;
        private readonly IPaymentsRepository _paymentsRepository;

        public OrderPaymentService(
            IDatabaseTask databaseTask,
            IOrdersRepository ordersRepository, 
            IPaymentsRepository paymentsRepository)
        {
            _databaseTask = databaseTask;
            _ordersRepository = ordersRepository;
            _paymentsRepository = paymentsRepository;
        }

        public async Task CompleteSuccess(long orderId)
        {
            await _databaseTask.ExecuteAsync(async (unitOfWork) =>
            {
                // If any of these operations throws an exception, the unitOfWork.Transaction is rolled back
                var paymentId = await _ordersRepository.GetPayment(unitOfWork, orderId).ConfigureAwait(false);
                await _paymentsRepository.UpdateStatus(unitOfWork, paymentId, "Success").ConfigureAwait(false);
                await _ordersRepository.UpdateStatus(unitOfWork, orderId, "Success").ConfigureAwait(false);
            }).ConfigureAwait(false);
        }
    }
``` 

## Sagas

**Sagas** allow us to define a business process as a series of steps that must all succeed or fail together.  Unlike a Unit of Work,
the steps of a Saga can cross application domains.  A prime example of this is making API calls to different microservices.

To create a saga, we derive from the abstract base class `Saga<T>`, where `T` is a user defined class to provide context to
each step of the Saga.

We add steps to the Saga with the _Configure_ method, typically in the constructor.

```CSharp
    public class CompleteOrderContext
    {
        public long OrderId { get; set; }
        public long PaymentId { get; set; }
    }

    public class CompleteOrderSaga : Saga<CompleteOrderContext>
    {
        public CompleteOrderSaga(IRestClient client)
        {
            // The ctx object is an ISagaConfiguration
            // and provides methods for adding steps.
            // In this example we'll provide inline lambda's
            this.Configure(ctx =>
            {
                // Each step is a pair of functions.  The first is the operation itself
                // and the second is to rollback the step in the case of an exception.
                // Each method receives the instance of the context object.
                // The rollback is optional
                ctx.AddStep(
                    async (context) =>
                    {
                        // Call the orders microservice to get the payment ID
                        context.PaymentId = await client.Get($"https://orders.example.com/orders/{context.OrderId}/payment").ConfigureAwait(false);
                        return context;
                    }
                    // There aren't any side effects of this call, so we don't need a Rollback
                );

                ctx.AddStep(
                    async (context) =>
                    {
                        // Call payment service to update status
                        await client.Patch($"https://payments.example.com/payments/{context.PaymentId}", new { Status = "Success" }).ConfigureAwait(false);
                        return context;
                    },
                    async (context) =>
                    {
                        // This is the rollback method and is only called if this step, or a later step throws an exception
                        await client.Patch($"https://payments.example.com/payments/{context.PaymentId}", new { Status = "Pending" }).ConfigureAwait(false);
                        return context;
                    } 
                );

                ctx.AddStep(
                    async (context) =>
                    {
                        // Call order service to update status
                        await client.Patch($"https://orders.example.com/orders/{context.OrderId}", new { Status = "Success" }).ConfigureAwait(false);
                        return context;
                    },
                    async (context) =>
                    {
                        await client.Patch($"https://orders.example.com/orders/{context.OrderId}", new { Status = "Pending" }).ConfigureAwait(false);
                        return context;
                    } 
                );
            });
        }
    }

    // To use the Saga, we instantiate it and invoke the Run method
    await new CompleteOrderSaga(GetRestClient())
                .Run(new CompleteOrderContext 
                        { 
                            OrderId = 12345 
                        })
                        .ConfigureAwait(false);
```

#### Combining Sagas and UnitOfWork

Sometimes we have a combination of steps that run in the same application domain and across application domains.  In this
case, we can derive from `UnitOfWorkSaga<T>` rather than `Saga<T>`. The steps of a `UnitOfWorkSaga`
receive an instance of `IUnitOfWork` as the second parameter.

```CSharp

    // ...
    ctx.AddStep(
        async (context, unitOfWork) =>
        {
            // use the unitOfWork as needed
        });
    // ...
```

In addition to the rollback method being invoked for each step, the `IUnitOfWork` will be rolled back for the entire saga, in
the case of an exception.

#### Known Limitations of Sagas

* If a rollback step also throws an exception, execution of the saga stops and the exception is thrown.  This could leave things
in an inconsistent state.  You should implement logging or another mechanism to handle this scenario. Note, however, that in the case
of a `UnitOfWorkSaga`, the `IDbTransaction` will be _always_ be rolled back.