using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;

namespace MVCForum.Data.Repositories
{
    public class CategoryPermissionForRoleRepository : ICategoryPermissionForRoleRepository
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
           categoryPermissionForRole.Id = Guid.NewGuid();
            return _context.CategoryPermissionForRole.Add(categoryPermissionForRole);
        }

        public CategoryPermissionForRole GetByPermissionRoleCategoryId(Guid permId, Guid roleId, Guid catId)
        {
                return _context.CategoryPermissionForRole.SingleOrDefault(x => x.Category.Id == catId && 
                                                                               x.Permission.Id == permId && 
                                                                               x.MembershipRole.Id == roleId);
        }

        public IList<CategoryPermissionForRole> GetCategoryRow(MembershipRole role, Category cat)
        {
            return _context.CategoryPermissionForRole
                .Where(x => x.Category.Id == cat.Id &&
                            x.MembershipRole.Id == role.Id)
                            .ToList();
        }

        public IEnumerable<CategoryPermissionForRole> GetByCategory(Guid catgoryId)
        {
            return _context.CategoryPermissionForRole
                .Where(x => x.Category.Id == catgoryId)
                .ToList();
        }

        public IEnumerable<CategoryPermissionForRole> GetByRole(Guid roleId)
        {
            return _context.CategoryPermissionForRole
                .Where(x => x.MembershipRole.Id == roleId)
                .ToList();
        }

        public IEnumerable<CategoryPermissionForRole> GetByPermission(Guid permId)
        {
            return _context.CategoryPermissionForRole
                .Where(x => x.Permission.Id == permId)
                .ToList();
        }

        public CategoryPermissionForRole Get(Guid id)
        {
            return _context.CategoryPermissionForRole.FirstOrDefault(cat => cat.Id == id);
        }

        public void Delete(CategoryPermissionForRole categoryPermissionForRole)
        {
            _context.CategoryPermissionForRole.Remove(categoryPermissionForRole);
        }

        public void Update(CategoryPermissionForRole item)
        {
            // Check there's not an object with same identifier already in context
            if (_context.CategoryPermissionForRole.Local.Select(x => x.Id == item.Id).Any())
            {
                throw new ApplicationException("Object already exists in context - you do not need to call Update. Save occurs on Commit");
            }
            _context.Entry(item).State = EntityState.Modified;  

        }
    }
}
