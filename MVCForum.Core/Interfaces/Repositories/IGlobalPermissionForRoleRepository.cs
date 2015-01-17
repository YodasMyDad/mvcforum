using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public partial interface IGlobalPermissionForRoleRepository
    {
        void Add(GlobalPermissionForRole permissionForRole);
        void Delete(GlobalPermissionForRole permissionForRole);
        GlobalPermissionForRole Get(Guid permId, Guid roleId);
        GlobalPermissionForRole Get(Guid permId);
        IList<GlobalPermissionForRole> GetAll(MembershipRole role);
        IList<GlobalPermissionForRole> GetAll();
    }
}
