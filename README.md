# Unit Of Work and Sagas
[![Build status](https://ci.appveyor.com/api/projects/status/000g0ngc7jkf58sx?svg=true)](https://ci.appveyor.com/project/orangeloop/sagas)

## Installation

##### .Net Core CLI
`dotnet add package OrangeLoop.Sagas`

##### Package Manager Console

`Install-Package OrangeLoop.Sagas`

## Unit Of Work

When using with a Dependency Injection framework, three interfaces
need to be registered:  `IUnitOfWorkFactory`, `IUnitOfWorkConfig` and `IConnectionStringFactory`.

Implementations for the first two are included. You will need to provide an implementation
of the `IConnectionStringFactory`.  

Two abstract classes are included to make reading from the `ConnectionStrings` section
of an application's configuration easier:

 * `AppSettingsConnectionStringFactory`
   * For loading from _appsettings.json_
 * `ConfigurationManagerConnectionStringFactory`
   * For loading from _app.config_ or _web.config_

##### Example:

###### appsettings.json

```json
{
    "ConnectionStrings": {
      "MyDB": "[Connection String]"
     }    
}
```

###### MyDBConnectionStringFactory.cs

```CSharp
public class MyDBConnectionStringFactory : AppSettingsConnectionStringFactory
{
    protected override string ConnectionName => "MyDB";
}
```

### Usage

The quickest way to get started is with the `DatabaseTask` class.  This class provides two methods: `ExecuteAsync` and `ExecuteAsync<T>`.
Each of these methods will ensure the database connection is open, and that your code
is executed within a transaction.

```CSharp
// First, create a DatabaseUnitOfWorkFactory.
// In this example, we're working with a SqlConnection
var factory = new DatabaseUnitOfWorkFactory<SqlConnection>(
   new MyDBConnectionStringFactory(),
   new DefaultConfig() // Sets IsolationLevel to ReadUncommited 
);

// Next, create a DatabaseTask
var task = new DatabaseTask(factory);

// Finally, execute business logic code
await task.ExecuteAsync((unitOfWork) => 
{
    // We can now access the unitOfWork.Transaction
    // For example, if we're using Dapper
    var connection = unitOfWork.Transaction.Connection;
    var results = await connection.QueryAsync<dynamic>("SELECT * FROM SomeTable", null, unitOfWork.Transaction).ConfigureAwait();

}).ConfigureAwait(false)
```


