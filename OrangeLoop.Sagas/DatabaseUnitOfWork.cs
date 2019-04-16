using OrangeLoop.Sagas.Interfaces;
using System;
using System.Data;

namespace OrangeLoop.Sagas
{
    public class DatabaseUnitOfWork : IUnitOfWork
    {
        public DatabaseUnitOfWork(IDbConnection connection, IUnitOfWorkConfig config)
        {
            Transaction = connection.BeginTransaction(config.IsolationLevel);
        }

        public IDbTransaction Transaction { get; private set; }

        public void Commit()
        {
            try
            {
                Transaction.Commit();
                Transaction.Connection?.Close();
            }
            catch(Exception)
            {
                Transaction.Rollback();
                throw;
            }
            finally
            {
                Transaction?.Dispose();
                Transaction?.Connection?.Dispose();
                Transaction = null;
            }
        }

        public void Rollback()
        {
            try
            {
                Transaction.Rollback();
                Transaction.Connection?.Close();
            }
            catch
            {
                throw;
            }
            finally
            {
                Transaction?.Dispose();
                Transaction?.Connection?.Dispose();
                Transaction = null;
            }
        }

        public void Dispose()
        {
            if (Transaction?.Connection?.State == System.Data.ConnectionState.Open)
            {
                Rollback();
            }
        }
    }
}
