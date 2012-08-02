using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public interface IPermissionService
    {
        IEnumerable<Permission> GetAll();
        void Add(Permission permission);
        void Delete(Permission permission);
        Permission Get(Guid id);
    }
}
