using OrangeLoop.Sagas.Interfaces;
using System.Data;
using System.Data.SqlClient;

namespace OrangeLoop.Sagas.UnitOfWork.SqlServer
{
    public sealed class SqlConnectionFactory(IConnectionStringFactory connectionStringFactory) : IConnectionFactory, IDisposable, IAsyncDisposable
    {
        private readonly SqlConnection _connection = new(connectionStringFactory.Get());
        private bool _disposed;

        public IDbConnection Get() => Task.Run(() => GetAsync()).Result;

        public async Task<IDbConnection> GetAsync()
        {
            if (_connection.State == ConnectionState.Closed)
            {
                await _connection.OpenAsync();
            }

            return _connection;
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_connection != null)
                {
                    if (_connection.State != ConnectionState.Closed)
                    {
                        _connection.Close();
                    }

                    _connection.Dispose();
                }

                _disposed = true;
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (!_disposed)
            {
                if (_connection != null)
                {
                    if (_connection.State != ConnectionState.Closed)
                    {
                        await _connection.CloseAsync();
                    }

                    await _connection.DisposeAsync();
                }

                _disposed = true;
            }
        }
    }
}
