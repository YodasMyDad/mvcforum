using MVCForum.Domain.Constants;
using System.Collections.Generic;

namespace MVCForum.Domain.DomainModel
{
    public partial class PermissionSet : Dictionary<string, PermissionForRole>
    {
        public PermissionSet(IEnumerable<CategoryPermissionForRole> categoryPermissions, IEnumerable<GlobalPermissionForRole> globalPermissions)
        {
            // Keep track of whether any permissions are true - if not, set Deny Access - this has the affect of inclusive access
            bool denyAccess = true;

            // Add the category permissions
            foreach (var categoryPermissionForRole in categoryPermissions)
            {
                // Due to multiple potential permissions across roles, check to see if permission exists before either adding it, or updating it
                if (!this.ContainsKey(categoryPermissionForRole.Permission.Name))
                {
                    // Add permission for action, if it doesn't already exist
                    if (categoryPermissionForRole.Permission.Name != SiteConstants.Instance.PermissionDenyAccess)
                    {
                        Add(categoryPermissionForRole.Permission.Name, MapCategoryPermission(categoryPermissionForRole));
                        if (categoryPermissionForRole.IsTicked)
                        {
                            denyAccess = false;
                        }
                    } else { 
                        Add(categoryPermissionForRole.Permission.Name, MapCategoryPermission(categoryPermissionForRole));
                    }
                } else {
                    if (categoryPermissionForRole.Permission.Name != SiteConstants.Instance.PermissionDenyAccess)
                    {
                        if (!this[categoryPermissionForRole.Permission.Name].IsTicked && categoryPermissionForRole.IsTicked)
                        {
                            this[categoryPermissionForRole.Permission.Name] = MapCategoryPermission(categoryPermissionForRole);
                            denyAccess = false;
                        }
                    }
                }
            }

            // If no other permissions are true, set Deny Access to true - this effectly Denies Access, unless another permission is granted.
            // This then negates the need to Deny Acccess in role permissions and means new categories are automatically denied access by default
            if (this.Count > 0)
            {
                if (denyAccess)
                {
                    this[SiteConstants.Instance.PermissionDenyAccess].IsTicked = true;
                }
                else
                {
                    this[SiteConstants.Instance.PermissionDenyAccess].IsTicked = false;
                }
            }

            // Add the global permissions
            foreach (var globalPermissionForRole in globalPermissions)
            {
                // Due to multiple potential permissions across roles, check to see if permission exists before either adding it, or updating it
                if (!this.ContainsKey(globalPermissionForRole.Permission.Name))
                {
                    Add(globalPermissionForRole.Permission.Name, MapGlobalPermission(globalPermissionForRole));
                } else
                {
                    if (!this[globalPermissionForRole.Permission.Name].IsTicked)
                    {
                        this[globalPermissionForRole.Permission.Name] = MapGlobalPermission(globalPermissionForRole);
                    }
                }
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
