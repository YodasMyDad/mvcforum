using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public interface IRoleRepository
    {
        IList<MembershipRole> AllRoles();
        MembershipRole GetRole(string rolename);

        MembershipRole Add(MembershipRole item);
        MembershipRole Get(Guid id);
        void Delete(MembershipRole item);
        void Update(MembershipRole item);
    }
}
