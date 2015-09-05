using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;

namespace MVCForum.Data.Repositories
{
    public partial class CategoryPermissionForRoleRepository : ICategoryPermissionForRoleRepository
    {
        private readonly MVCForumContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        public CategoryPermissionForRoleRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        public CategoryPermissionForRole Add(CategoryPermissionForRole categoryPermissionForRole)
        {
            return _context.CategoryPermissionForRole.Add(categoryPermissionForRole);
        }

        public CategoryPermissionForRole GetByPermissionRoleCategoryId(Guid permId, Guid roleId, Guid catId)
        {
            return _context.CategoryPermissionForRole
                .Include(x => x.MembershipRole)
                .Include(x => x.Category)
                .FirstOrDefault(x => x.Category.Id == catId && 
                                     x.Permission.Id == permId && 
                                     x.MembershipRole.Id == roleId);
        }

        public IList<CategoryPermissionForRole> GetCategoryRow(MembershipRole role, Category cat)
        {
            return _context.CategoryPermissionForRole
                .Include(x => x.MembershipRole)
                .Include(x => x.Category)
                .AsNoTracking()
                .Where(x => x.Category.Id == cat.Id &&
                            x.MembershipRole.Id == role.Id)
                            .ToList();
        }

        public IEnumerable<CategoryPermissionForRole> GetByCategory(Guid catgoryId)
        {
            return _context.CategoryPermissionForRole
                .Include(x => x.MembershipRole)
                .Include(x => x.Category)
                .Where(x => x.Category.Id == catgoryId)
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
