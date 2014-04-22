using System;
using System.Collections.Generic;
using System.Linq;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Services
{
    public partial class CategoryPermissionForRoleService : ICategoryPermissionForRoleService
    {
        private readonly ICategoryPermissionForRoleRepository _categoryPermissionForRoleService;

        public CategoryPermissionForRoleService(ICategoryPermissionForRoleRepository categoryPermissionForRoleService)
        {
            _categoryPermissionForRoleService = categoryPermissionForRoleService;
        }

        /// <summary>
        /// Add new category permission for role
        /// </summary>
        /// <param name="categoryPermissionForRole"></param>
        public void Add(CategoryPermissionForRole categoryPermissionForRole)
        {
            _categoryPermissionForRoleService.Add(categoryPermissionForRole);
        }

        /// <summary>
        /// Check the category permission for role actually exists
        /// </summary>
        /// <param name="categoryPermissionForRole"></param>
        /// <returns></returns>
        public CategoryPermissionForRole CheckExists(CategoryPermissionForRole categoryPermissionForRole)
        {
            if (categoryPermissionForRole.Permission != null &&
                    categoryPermissionForRole.Category != null &&
                        categoryPermissionForRole.MembershipRole != null)
            {

                return _categoryPermissionForRoleService.GetByPermissionRoleCategoryId(categoryPermissionForRole.Permission.Id,
                                                          categoryPermissionForRole.MembershipRole.Id,
                                                          categoryPermissionForRole.Category.Id);
            }

            return null;
        }

        /// <summary>
        /// Either updates a CPFR if exists or creates a new one
        /// </summary>
        /// <param name="categoryPermissionForRole"></param>
        public void UpdateOrCreateNew(CategoryPermissionForRole categoryPermissionForRole)
        {
            // Firstly see if this exists already
            var permission = CheckExists(categoryPermissionForRole);

            // if it exists then just update it
            if (permission != null)
            {
                permission.IsTicked = categoryPermissionForRole.IsTicked;
            }
            else
            {
                Add(categoryPermissionForRole);
            }
        }

        /// <summary>
        /// Saves/Updates CPFR
        /// </summary>
        /// <param name="categoryPermissionForRole"></param>
        public void Save(CategoryPermissionForRole categoryPermissionForRole)
        {
            _categoryPermissionForRoleService.Update(categoryPermissionForRole); 
        }

        /// <summary>
        /// Returns a row with the permission and CPFR
        /// </summary>
        /// <param name="role"></param>
        /// <param name="cat"></param>
        /// <returns></returns>
        public Dictionary<Permission, CategoryPermissionForRole> GetCategoryRow(MembershipRole role, Category cat)
        {
            var catRowList = _categoryPermissionForRoleService.GetCategoryRow(role, cat);
            return catRowList.ToDictionary(catRow => catRow.Permission);
        }

        /// <summary>
        /// Get all category permissions by category
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public IEnumerable<CategoryPermissionForRole> GetByCategory(Guid categoryId)
        {
            return _categoryPermissionForRoleService.GetByCategory(categoryId);
        }
    }
}
