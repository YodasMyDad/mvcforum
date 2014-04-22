using System;

namespace MVCForum.Domain.Interfaces.UnitOfWork
{
    public partial interface IUnitOfWork : IDisposable
    {
        void Commit();
        void Rollback();
        void SaveChanges();
    }
}
