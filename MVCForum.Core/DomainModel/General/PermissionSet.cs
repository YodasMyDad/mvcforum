using System.Collections.Generic;

namespace MVCForum.Domain.DomainModel
{
    public partial class PermissionSet : Dictionary<string, PermissionForRole>
    {
        public PermissionSet(IEnumerable<CategoryPermissionForRole> categoryPermissions, IEnumerable<GlobalPermissionForRole> globalPermissions)
        {
            // Add the category permissions
            foreach (var categoryPermissionForRole in categoryPermissions)
            {
                Add(categoryPermissionForRole.Permission.Name, MapCategoryPermission(categoryPermissionForRole));
            }

            // Add the global permissions
            foreach (var globalPermissionForRole in globalPermissions)
            {
                Add(globalPermissionForRole.Permission.Name, MapGlobalPermission(globalPermissionForRole));
            }
        }

        private static PermissionForRole MapCategoryPermission(CategoryPermissionForRole cp)
        {
            var pfr = new PermissionForRole
            {
                Category = cp.Category,
                IsTicked = cp.IsTicked,
                MembershipRole = cp.MembershipRole,
                Permission = cp.Permission
            };

            return pfr;
        }

        private static PermissionForRole MapGlobalPermission(GlobalPermissionForRole gp)
        {
            var pfr = new PermissionForRole
            {
                IsTicked = gp.IsTicked,
                MembershipRole = gp.MembershipRole,
                Permission = gp.Permission
            };

            return pfr;
        }
    }
}
