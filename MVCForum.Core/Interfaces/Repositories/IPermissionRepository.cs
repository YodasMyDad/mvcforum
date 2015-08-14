using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public partial interface IPermissionRepository
    {
        IEnumerable<Permission> GetAll();
        Permission Add(Permission item);
        Permission Get(Guid id);
        void Delete(Permission item);
    }
}
