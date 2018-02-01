namespace MvcForum.Core.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using Models.Entities;

    public partial interface ICategoryPermissionForRoleService : IContextService
    {
        /// <summary>
        ///     Add new category permission for role
        /// </summary>
        /// <param name="categoryPermissionForRole"></param>
        CategoryPermissionForRole Add(CategoryPermissionForRole categoryPermissionForRole);

        /// <summary>
        ///     Check the category permission for role actually exists
        /// </summary>
        /// <param name="categoryPermissionForRole"></param>
        /// <returns></returns>
        CategoryPermissionForRole CheckExists(CategoryPermissionForRole categoryPermissionForRole);

        /// <summary>
        ///     Either updates a CPFR if exists or creates a new one
        /// </summary>
        /// <param name="categoryPermissionForRole"></param>
        void UpdateOrCreateNew(CategoryPermissionForRole categoryPermissionForRole);

        /// <summary>
        ///     Returns a row with the permission and CPFR
        /// </summary>
        /// <param name="role"></param>
        /// <param name="cat"></param>
        /// <returns></returns>
        Dictionary<Permission, CategoryPermissionForRole> GetCategoryRow(MembershipRole role, Category cat);

        /// <summary>
        ///     Get all category permissions by category
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        IEnumerable<CategoryPermissionForRole> GetByCategory(Guid categoryId);

        IEnumerable<CategoryPermissionForRole> GetByRole(Guid roleId);
        IEnumerable<CategoryPermissionForRole> GetByPermission(Guid permId);
        CategoryPermissionForRole Get(Guid id);
        void Delete(CategoryPermissionForRole categoryPermissionForRole);
    }
}