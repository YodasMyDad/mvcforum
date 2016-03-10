using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Services.Data.Context;

namespace MVCForum.Services
{
    public partial class CategoryPermissionForRoleService : ICategoryPermissionForRoleService
    {
        private readonly MVCForumContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public CategoryPermissionForRoleService(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        /// <summary>
        /// Add new category permission for role
        /// </summary>
        /// <param name="categoryPermissionForRole"></param>
        public CategoryPermissionForRole Add(CategoryPermissionForRole categoryPermissionForRole)
        {
            return _context.CategoryPermissionForRole.Add(categoryPermissionForRole);
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

                return _context.CategoryPermissionForRole
                        .Include(x => x.MembershipRole)
                        .Include(x => x.Category)
                        .FirstOrDefault(x => x.Category.Id == categoryPermissionForRole.Category.Id &&
                                             x.Permission.Id == categoryPermissionForRole.Permission.Id &&
                                             x.MembershipRole.Id == categoryPermissionForRole.MembershipRole.Id);
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
        /// Returns a row with the permission and CPFR
        /// </summary>
        /// <param name="role"></param>
        /// <param name="cat"></param>
        /// <returns></returns>
        public Dictionary<Permission, CategoryPermissionForRole> GetCategoryRow(MembershipRole role, Category cat)
        {
            var catRowList = _context.CategoryPermissionForRole
                            .Include(x => x.MembershipRole)
                            .Include(x => x.Category)
                            .AsNoTracking()
                            .Where(x => x.Category.Id == cat.Id &&
                                        x.MembershipRole.Id == role.Id)
                                        .ToList();
            return catRowList.ToDictionary(catRow => catRow.Permission);
        }

        /// <summary>
        /// Get all category permissions by category
        /// </summary>
        /// <param name="categoryId"></param>
        /// <returns></returns>
        public IEnumerable<CategoryPermissionForRole> GetByCategory(Guid categoryId)
        {
            return _context.CategoryPermissionForRole
                .Include(x => x.MembershipRole)
                .Include(x => x.Category)
                .Where(x => x.Category.Id == categoryId)
                .ToList();
        }

        public IEnumerable<CategoryPermissionForRole> GetByRole(Guid roleId)
        {
            return _context.CategoryPermissionForRole
                .Include(x => x.MembershipRole)
                .Include(x => x.Category)
                .Where(x => x.MembershipRole.Id == roleId)
                .ToList();
        }

        public IEnumerable<CategoryPermissionForRole> GetByPermission(Guid permId)
        {
            return _context.CategoryPermissionForRole
                .Include(x => x.MembershipRole)
                .Include(x => x.Category)
                .Where(x => x.Permission.Id == permId)
                .ToList();
        }

        public CategoryPermissionForRole Get(Guid id)
        {
            return _context.CategoryPermissionForRole
                    .Include(x => x.MembershipRole)
                    .Include(x => x.Category)
                    .FirstOrDefault(cat => cat.Id == id);
        }

        public void Delete(CategoryPermissionForRole categoryPermissionForRole)
        {
            _context.CategoryPermissionForRole.Remove(categoryPermissionForRole);
        }
    }
}
