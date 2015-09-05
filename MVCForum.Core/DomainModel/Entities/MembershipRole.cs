using System;
using System.Collections.Generic;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    public partial class MembershipRole : Entity
    {
        public MembershipRole()
        {
            Id = GuidComb.GenerateComb();
        }

        public Guid Id { get; set; }
        public string RoleName { get; set; }
        public virtual IList<MembershipUser> Users { get; set; }
        public virtual Settings Settings { get; set; }

        // Category Permissions
        public virtual IList<CategoryPermissionForRole> CategoryPermissionForRoles { get; set; }

        // Global Permissions
        public virtual IList<GlobalPermissionForRole> GlobalPermissionForRole { get; set; }

        public virtual Dictionary<Guid, Dictionary<Guid, bool>> GetCategoryPermissionTable()
        {
            var permissionRows = new Dictionary<Guid, Dictionary<Guid, bool>>();

            foreach (var catPermissionForRole in CategoryPermissionForRoles)
            {
                if (!permissionRows.ContainsKey(catPermissionForRole.Category.Id))
                {
                    var permissionList = new Dictionary<Guid, bool>();

                    permissionRows.Add(catPermissionForRole.Category.Id, permissionList);
                }

                if (!permissionRows[catPermissionForRole.Category.Id].ContainsKey(catPermissionForRole.Permission.Id))
                {
                    permissionRows[catPermissionForRole.Category.Id].Add(catPermissionForRole.Permission.Id, catPermissionForRole.IsTicked);
                }
                else
                {
                    permissionRows[catPermissionForRole.Category.Id][catPermissionForRole.Permission.Id] = catPermissionForRole.IsTicked;
                }
                

            }
            return permissionRows;
        }

    }
}
