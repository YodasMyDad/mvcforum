using System;
using System.Collections.Generic;

namespace MVCForum.Domain.Interfaces.UnitOfWork
{
    public partial interface IUnitOfWork : IDisposable
    {
        void Commit();
        void Commit(List<string> cacheStartsWithToClear);
        void Rollback();
        void SaveChanges();
        void AutoDetectChangesEnabled(bool option);
        void LazyLoadingEnabled(bool option);
    }
}
