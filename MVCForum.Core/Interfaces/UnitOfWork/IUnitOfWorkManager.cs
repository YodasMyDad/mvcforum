using System;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.UnitOfWork
{
    public interface IUnitOfWorkManager : IDisposable
    {
        IUnitOfWork NewUnitOfWork();        
    }
}
