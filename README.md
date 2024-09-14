# OrangeLoop.Sagas

[![Build Status](https://dev.azure.com/livefree/engineering/_apis/build/status%2FOrangeloop%2FOrangeloop.Sagas?branchName=develop)](https://dev.azure.com/livefree/engineering/_build/latest?definitionId=128&branchName=develop)

> [!WARNING]
> Version 2 of this package is a complete refactor and is **NOT** backwards compatible.
> This version targets **net80** and has a much easier interface to work with than
> version 1.

## Installation

##### .Net Core CLI

`dotnet add package OrangeLoop.Sagas`

##### Package Manager Console

`Install-Package OrangeLoop.Sagas`

## How to get started with `IUnitOfWork` (SQL Server)

### Add connection string

```JSON
// appsettings.json

{
  "ConnectionStrings": {
    "MyConnection": "Data Source=...",
  },
  ...
}
```

### Register `IUnitOfWork` Service

```CSharp
// Startup.cs or Program.cs

services.AddSqlServerUnitOfWork("MyConnection", IsolationLevel.ReadUncommitted);

```

### Inject `IConnectionFactory` and `IUnitOfWork` in repository

Database queries will need a reference to the current `IDbTransaction`, which can be accessed via the `CurrentTransaction` property of `IUnitOfWork`. Libraries such as Dapper or RepoDB have a `transaction` parameter for this purpose.

#### Example

```CSharp
// ICustomersRepository.cs
public interface ICustomersRepository
{
    Task<Customer> Create(Customer customer);
    Task<Customer> Delete(Customer customer);
    Task<Customer> FindById(long id);
}

// CustomersRepository.cs
public class CustomersRepository(IConnectionFactory connectionFactory, IUnitOfWork unitOfWork) : ICustomersRepository
{
    public async Task<Customer> FindById(long id)
    {
        var conn = connectionFactory.Get();
        var result = await conn.QueryFirstOrDefaultAsync<Customer>("...", transaction: unitOfWork.CurrentTransaction);
    }
}
```

> [!NOTE] > `IConnectionFactory` is registered as a Scoped service and implements `IDisposable`.
> When the scope is disposed (e.g. after an ASP.NET Request) the underlying
> database connection is closed and properly disposed.

### `IUnitOfWork.ExecuteAsync` (Implicit)

When using the implicit option, if no exceptions are thrown, the transaction is committed. If an unhandled exception is thrown, the transaction will be rolled back.

```CSharp
// CustomersService.cs

public class CustomersService(IUnitOfWork unitOfWork, ICustomersRepository repo) : ICustomersService
{
    public async Task SomeMethod()
    {
        await unitOfWork.ExecuteAsync(async () =>
        {
            await repo.Create(...);
            await repo.Create(...);
            await repo.Delete(...);
        });
    }
}
```

### `IUnitOfWork.ExecuteAsync` (Explicit)

Usually the implicit option is best, but if you want to handle exceptions within the ExecuteAsync method, then using the explicit option provides that flexibility. Alternatively you can use the implict option and simply rethrow the exception.

```CSharp
// CustomersService.cs

public class CustomersService(IUnitOfWork unitOfWork, ICustomersRepository repo) : ICustomersService
{
    public async Task SomeMethod()
    {
        await unitOfWork.ExecuteAsync(async (success, failure) =>
        {
            try
            {
                await repo.Create(...);
                await repo.Create(...);
                await repo.Delete(...);
                await success();
            }
            catch(Exception e)
            {
                // Custom exception handling
                await failure(e);
            }
        });
    }
}
```

> [!WARNING]
> Failure to call `success` or `failure` when using the explicit option can lead
> to open database transactions. I will address this in a future update.

## How to get started with `ISaga<T>`

> [!NOTE]
> Documentation pending. Sample usage available in `SagaTests.cs`
