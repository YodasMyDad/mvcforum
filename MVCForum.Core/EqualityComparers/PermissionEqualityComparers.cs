using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.EqualityComparers
{
    public class PermissionEqualityComparer : IEqualityComparer<Permission>
    {
        public bool Equals(Permission x, Permission y)
        {
            return (x.Id == y.Id);
        }

        public int GetHashCode(Permission obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
