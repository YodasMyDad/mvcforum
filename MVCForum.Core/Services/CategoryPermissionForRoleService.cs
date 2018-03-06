namespace MvcForum.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Constants;
    using Interfaces;
    using Interfaces.Services;
    using Models.Entities;

    public partial class CategoryPermissionForRoleService : ICategoryPermissionForRoleService
    {
        private IMvcForumContext _context;
        private readonly ICacheService _cacheService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="cacheService"></param>
        public CategoryPermissionForRoleService(IMvcForumContext context, ICacheService cacheService)
        {
            _cacheService = cacheService;
            _context = context;
        }

        /// <inheritdoc />
        public void RefreshContext(IMvcForumContext context)
        {
            _context = context;
        }

        /// <inheritdoc />
        public async Task<int> SaveChanges()
        {
            return await _context.SaveChangesAsync();
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
                .Include(x => x.Permission)
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
                                .Include(x => x.Permission)
                                .Where(x => x.Category.Id == categoryId)
                                .ToList();
        }

        public IEnumerable<CategoryPermissionForRole> GetByRole(Guid roleId)
        {
                return _context.CategoryPermissionForRole
                    .Include(x => x.MembershipRole)
                    .Include(x => x.Category)
                    .Include(x => x.Permission)
                    .Where(x => x.MembershipRole.Id == roleId);
        }

        public IEnumerable<CategoryPermissionForRole> GetByPermission(Guid permId)
        {

                return _context.CategoryPermissionForRole
                    .Include(x => x.MembershipRole)
                    .Include(x => x.Category)
                    .Include(x => x.Permission)
                    .Where(x => x.Permission.Id == permId);

        }

        public CategoryPermissionForRole Get(Guid id)
        {

                return _context.CategoryPermissionForRole
                        .Include(x => x.MembershipRole)
                        .Include(x => x.Category)
                        .Include(x => x.Permission)
                        .FirstOrDefault(cat => cat.Id == id);
      

        }

        public void Delete(CategoryPermissionForRole categoryPermissionForRole)
        {
            _context.CategoryPermissionForRole.Remove(categoryPermissionForRole);
        }
    }
}
