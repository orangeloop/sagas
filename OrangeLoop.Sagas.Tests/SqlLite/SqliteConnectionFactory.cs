using OrangeLoop.Sagas.Interfaces;
using System;
using System.Data;
using System.Data.SQLite;
using System.Threading.Tasks;

namespace OrangeLoop.Sagas.Tests.SqlLite
{
    public class SqliteConnectionFactory(IConnectionStringFactory connectionStringFactory) : IConnectionFactory, IDisposable, IAsyncDisposable
    {
        private SQLiteConnection _connection = new SQLiteConnection(connectionStringFactory.Get());

        public void Dispose()
            => Task.Run(() => DisposeAsync());

        public async ValueTask DisposeAsync()
        {
            if (_connection.State != ConnectionState.Open)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }
        }

        public IDbConnection Get()
            => Task.Run(() => GetAsync()).Result;

        public async Task<IDbConnection> GetAsync()
        {
            if (_connection.State == ConnectionState.Closed)
            {
                await _connection.OpenAsync();
            }

            return _connection;
        }
    }
}
