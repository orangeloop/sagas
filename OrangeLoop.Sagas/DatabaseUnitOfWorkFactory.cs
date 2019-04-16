using OrangeLoop.Sagas.Interfaces;
using System;
using System.Data;

namespace OrangeLoop.Sagas
{
    public class DatabaseUnitOfWorkFactory<T> : IUnitOfWorkFactory where T : IDbConnection, new()
    {
        private readonly string _connectionString;
        private readonly IUnitOfWorkConfig _config;

        public DatabaseUnitOfWorkFactory(IConnectionStringFactory connectionStringFactory, IUnitOfWorkConfig config)
        {
            _connectionString = connectionStringFactory.Get();
            _config = config;
        }

        public IUnitOfWork Create()
        {
            return new DatabaseUnitOfWork(CreateConnection(), _config);
        }

        private IDbConnection CreateConnection()
        {
            var conn = new T()
            {
                ConnectionString = _connectionString
            };
             
            try
            {
                if (conn.State != ConnectionState.Open)
                {
                    conn.Open();
                }
            }
            catch (Exception exception)
            {
                throw new Exception("Unable to connect to database.", exception);
            }

            return conn;
        }
    }
}
