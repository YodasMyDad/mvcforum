using System;

namespace MVCForum.Domain.Interfaces.UnitOfWork
{
    public interface IUnitOfWork : IDisposable
    {
        void Commit();
        void Rollback();
        void SaveChanges();
    }
}
