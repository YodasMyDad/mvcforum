using System;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.UnitOfWork
{
    public partial interface IUnitOfWorkManager : IDisposable
    {
        //IUnitOfWork NewUnitOfWork(bool isReadyOnly);     
        IUnitOfWork NewUnitOfWork();
    }
}
