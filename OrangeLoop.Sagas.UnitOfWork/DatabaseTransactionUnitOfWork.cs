using OrangeLoop.Sagas.Interfaces;
using System.Data;

namespace OrangeLoop.Sagas.UnitOfWork
{
    public class DatabaseTransactionUnitOfWork(IConnectionFactory connectionFactory, IDatabaseTransactionConfiguration transactionConfiguration)
        : BaseDatabaseTransactionUnitOfWork(connectionFactory)
    {
        public override IDbTransaction? CurrentTransaction { get; protected set; }

        public override Task CommitAsync()
            => Task.Run(() =>
            {
                if (CurrentTransaction != null)
                {
                    CurrentTransaction.Commit();
                    CurrentTransaction.Dispose();
                    CurrentTransaction = null;
                }
            });

        public override Task RollbackAsync()
            => Task.Run(() =>
            {
                if (CurrentTransaction != null)
                {
                    CurrentTransaction.Rollback();
                    CurrentTransaction.Dispose();
                    CurrentTransaction = null;
                }
            });

        public override Task StartTransactionAsync()
            => Task.Run(() =>
            {
                CurrentTransaction ??= Connection.BeginTransaction(transactionConfiguration.IsolationLevel);
            });
    }
}
