using System;
using System.Data;

namespace OrangeLoop.Sagas.Interfaces
{
    public interface IUnitOfWork : IDisposable
    {
        IDbTransaction Transaction { get; }
        void Commit();
        void Rollback();
    }
}
