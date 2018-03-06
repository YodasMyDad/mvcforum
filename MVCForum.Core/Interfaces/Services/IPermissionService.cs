namespace MvcForum.Core.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using Models.Entities;

    public partial interface IPermissionService : IContextService
    {
        IEnumerable<Permission> GetAll();
        Permission Add(Permission permission);
        void Delete(Permission permission);
        Permission Get(Guid id);
    }
}