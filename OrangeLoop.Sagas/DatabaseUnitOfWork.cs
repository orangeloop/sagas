using OrangeLoop.Sagas.Interfaces;
using System;
using System.Data;

namespace OrangeLoop.Sagas
{
    public class DatabaseUnitOfWork : IUnitOfWork
    {
        public DatabaseUnitOfWork(IDbConnection connection, IUnitOfWorkConfig config)
        {
            Connection = connection;
            Transaction = connection.BeginTransaction(config.IsolationLevel);
        }

        public IDbTransaction Transaction { get; private set; }
        private IDbConnection Connection { get; set; }

        public void Commit()
        {
            try
            {
                Transaction.Commit();
                Connection?.Close();
            }
            catch(Exception)
            {
                Transaction.Rollback();
                throw;
            }
            finally
            {
                Transaction?.Dispose();
                Connection?.Dispose();
                Transaction = null;
            }
        }

        public void Rollback()
        {
            try
            {
                Transaction.Rollback();
                Connection?.Close();
            }
            catch
            {
                throw;
            }
            finally
            {
                Transaction?.Dispose();
                Connection?.Dispose();
                Transaction = null;
            }
        }

        public void Dispose()
        {
            if (Connection?.State == System.Data.ConnectionState.Open)
            {
                Rollback();
            }
        }
    }
}
