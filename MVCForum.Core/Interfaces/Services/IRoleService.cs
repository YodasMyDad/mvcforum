namespace MvcForum.Core.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using Models.Entities;
    using Models.General;

    public partial interface IRoleService : IContextService
    {
        IList<MembershipRole> AllRoles();
        void Delete(MembershipRole role);
        MembershipRole GetRole(string roleName, bool removeTracking = false);
        MembershipRole GetRole(Guid id);
        IList<MembershipUser> GetUsersForRole(string roleName);
        MembershipRole CreateRole(MembershipRole role);
        PermissionSet GetPermissions(Category category, MembershipRole role);
    }
}
