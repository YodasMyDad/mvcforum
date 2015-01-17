using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public partial interface ICategoryPermissionForRoleRepository
    {
        CategoryPermissionForRole Add(CategoryPermissionForRole categoryPermissionForRole);
        CategoryPermissionForRole GetByPermissionRoleCategoryId(Guid permId, Guid roleId, Guid catId);
        IList<CategoryPermissionForRole> GetCategoryRow(MembershipRole role, Category cat);
        IEnumerable<CategoryPermissionForRole> GetByCategory(Guid catgoryId);
        IEnumerable<CategoryPermissionForRole> GetByRole(Guid roleId);
        IEnumerable<CategoryPermissionForRole> GetByPermission(Guid permId);
        CategoryPermissionForRole Get(Guid id);
        void Delete(CategoryPermissionForRole entity);
    }
}
