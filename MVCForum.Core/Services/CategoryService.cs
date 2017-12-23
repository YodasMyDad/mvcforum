namespace MVCForum.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Web;
    using System.Web.Mvc;
    using System.Data.Entity;
    using Domain.Constants;
    using Domain.DomainModel;
    using Domain.DomainModel.General;
    using Domain.Exceptions;
    using Domain.Interfaces;
    using Domain.Interfaces.Services;
    using Data.Context;
    using Utilities;

    public partial class CategoryService : ICategoryService
    {
        private readonly IRoleService _roleService;
        private readonly ICategoryNotificationService _categoryNotificationService;
        private readonly ICategoryPermissionForRoleService _categoryPermissionForRoleService;
        private readonly MVCForumContext _context;
        private readonly ICacheService _cacheService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="roleService"> </param>
        /// <param name="categoryNotificationService"> </param>
        /// <param name="categoryPermissionForRoleService"></param>
        /// <param name="cacheService"></param>
        public CategoryService(IMVCForumContext context, IRoleService roleService, ICategoryNotificationService categoryNotificationService, ICategoryPermissionForRoleService categoryPermissionForRoleService, ICacheService cacheService)
        {
            _roleService = roleService;
            _categoryNotificationService = categoryNotificationService;
            _categoryPermissionForRoleService = categoryPermissionForRoleService;
            _cacheService = cacheService;
            _context = context as MVCForumContext;
        }

        /// <summary>
        /// Return all categories
        /// </summary>
        /// <returns></returns>
        public List<Category> GetAll()
        {
            var cacheKey = string.Concat(CacheKeys.Category.StartsWith, "GetAll");
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                var orderedCategories = new List<Category>();
                var allCats = _context.Category
                        .Include(x => x.ParentCategory)
                        .AsNoTracking()
                        .OrderBy(x => x.SortOrder)
                        .ToList();

                foreach (var parentCategory in allCats.Where(x => x.ParentCategory == null).OrderBy(x => x.SortOrder))
                {
                    // Add the main category
                    parentCategory.Level = 1;
                    orderedCategories.Add(parentCategory);

                    // Add subcategories under this
                    orderedCategories.AddRange(GetSubCategories(parentCategory, allCats));
                }
                return orderedCategories;
            });
        }

        public List<Category> GetSubCategories(Category category, List<Category> allCategories, int level = 2)
        {
            var cacheKey = string.Concat(CacheKeys.Category.StartsWith, "GetSubCategories", "-", category.GetHashCode(), "-", level);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                var catsToReturn = new List<Category>();
                var cats = allCategories.Where(x => x.ParentCategory != null && x.ParentCategory.Id == category.Id).OrderBy(x => x.SortOrder);
                foreach (var cat in cats)
                {
                    cat.Level = level;
                    catsToReturn.Add(cat);
                    catsToReturn.AddRange(GetSubCategories(cat, allCategories, level + 1));
                }

                return catsToReturn;
            });
        }

        public List<SelectListItem> GetBaseSelectListCategories(List<Category> allowedCategories)
        {
            var cacheKey = string.Concat(CacheKeys.Category.StartsWith, "GetBaseSelectListCategories", "-", allowedCategories.GetHashCode());
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                var cats = new List<SelectListItem> { new SelectListItem { Text = "", Value = "" } };
                foreach (var cat in allowedCategories)
                {
                    var catName = string.Concat(LevelDashes(cat.Level), cat.Level > 1 ? " " : "", cat.Name);
                    cats.Add(new SelectListItem { Text = catName, Value = cat.Id.ToString() });
                }
                return cats;
            });
        }

        private static string LevelDashes(int level)
        {
            if (level > 1)
            {
                var sb = new StringBuilder();
                for (var i = 0; i < level-1; i++)
                {
                    sb.Append("-");
                }
                return sb.ToString();
            }
            return string.Empty;
        }

        /// <summary>
        /// Return all sub categories from a parent category id
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public IEnumerable<Category> GetAllSubCategories(Guid parentId)
        {
            var cacheKey = string.Concat(CacheKeys.Category.StartsWith, "GetAllSubCategories", "-", parentId);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                return _context.Category
                        .Where(x => x.ParentCategory.Id == parentId)
                        .OrderBy(x => x.SortOrder);
            });
        }

        /// <summary>
        /// Get all main categories (Categories with no parent category)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Category> GetAllMainCategories()
        {
            var cacheKey = string.Concat(CacheKeys.Category.StartsWith, "GetAllMainCategories");
            return _cacheService.CachePerRequest(cacheKey, () => _context.Category
                                                                        .Include(x => x.ParentCategory)
                                                                        .Include(x => x.Topics.Select(l => l.LastPost))
                                                                        .Include(x => x.Topics.Select(l => l.Posts))
                                                                        .Where(cat => cat.ParentCategory == null)
                                                                        .OrderBy(x => x.SortOrder)
                                                                        .ToList());
        }

        /// <summary>
        /// Return allowed categories based on the users role
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public List<Category> GetAllowedCategories(MembershipRole role)
        {
            return GetAllowedCategories(role, SiteConstants.Instance.PermissionDenyAccess);
        }

        public List<Category> GetAllowedCategories(MembershipRole role, string actionType)
        {
            return GetAllowedCategoriesCode(role, actionType);
        }

        private List<Category> GetAllowedCategoriesCode(MembershipRole role, string actionType)
        {
            var cacheKey = string.Concat(CacheKeys.Category.StartsWith, "GetAllowedCategoriesCode-", role.Id, "-", actionType);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                var filteredCats = new List<Category>();
                var allCats = GetAll();
                foreach (var category in allCats)
                {
                    var permissionSet = _roleService.GetPermissions(category, role);
                    if (!permissionSet[actionType].IsTicked)
                    {
                        // Only add it category is NOT locked
                        filteredCats.Add(category);
                    }
                }
                return filteredCats;
            });
        }

        /// <summary>
        /// Add a new category
        /// </summary>
        /// <param name="category"></param>
        public Category Add(Category category)
        {
            // Sanitize
            category = SanitizeCategory(category);

            // Set the create date
            category.DateCreated = DateTime.UtcNow;

            // url slug generator
            category.Slug = ServiceHelpers.GenerateSlug(category.Name, GetBySlugLike(ServiceHelpers.CreateUrl(category.Name)), null);

            // Add the category
            return _context.Category.Add(category);
        }

        /// <summary>
        /// Keep slug in line with name
        /// </summary>
        /// <param name="category"></param>
        public void UpdateSlugFromName(Category category)
        {
            // Sanitize
            category = SanitizeCategory(category);

            var updateSlug = true;

            // Check if slug has changed as this could be an update
            if (!string.IsNullOrEmpty(category.Slug))
            {
                var categoryBySlug = GetBySlugWithSubCategories(category.Slug);
                if (categoryBySlug.Category.Id == category.Id)
                {
                    updateSlug = false;
                }
            }

            if (updateSlug)
            {
                category.Slug = ServiceHelpers.GenerateSlug(category.Name, GetBySlugLike(category.Slug), category.Slug);   
            }
        }

        /// <summary>
        /// Sanitizes a category
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public Category SanitizeCategory(Category category)
        {
            // Sanitize any strings in a category
            category.Description = StringUtils.GetSafeHtml(category.Description);
            category.Name = HttpUtility.HtmlDecode(StringUtils.SafePlainText(category.Name));
            return category;
        }

        /// <summary>
        /// Return category by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Category Get(Guid id)
        {
            var cacheKey = string.Concat(CacheKeys.Category.StartsWith, "Get-", id);
            return _cacheService.CachePerRequest(cacheKey, () => _context.Category.FirstOrDefault(x => x.Id == id));
        }

        public IList<Category> Get(IList<Guid> ids, bool fullGraph = false)
        {
            var cacheKey = string.Concat(CacheKeys.Category.StartsWith, "Get-", ids.GetHashCode(), "-", fullGraph);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                IList<Category> categories;

                if (fullGraph)
                {
                    categories =
                        _context.Category.AsNoTracking()
                            .Include(x => x.Topics.Select(l => l.LastPost.User))
                            .Include(x => x.ParentCategory)
                            .Where(x => ids.Contains(x.Id))
                            .ToList();
                }
                else
                {
                    categories = _context.Category
                        .AsNoTracking().Where(x => ids.Contains(x.Id)).ToList();
                }

                // make sure categories are returned in order of ids (not in Database order)
                return ids.Select(id => categories.Single(c => c.Id == id)).ToList();
            });
        }

        /// <summary>
        /// Return model with Sub categories
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public CategoryWithSubCategories GetBySlugWithSubCategories(string slug)
        {
            slug = StringUtils.SafePlainText(slug);

            var cacheKey = string.Concat(CacheKeys.Category.StartsWith, "GetBySlugWithSubCategories-", slug);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                var cat = (from category in _context.Category
                           where category.Slug == slug
                           select new CategoryWithSubCategories
                           {
                               Category = category,
                               SubCategories = (from cats in _context.Category
                                                where cats.ParentCategory.Id == category.Id
                                                select cats)
                           }).FirstOrDefault();

                return cat;
            });
        }

        /// <summary>
        /// Return category by Url slug
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public Category Get(string slug)
        {
            return GetBySlug(slug);
        }

        /// <summary>
        /// Gets the category parents
        /// </summary>
        /// <param name="category"></param>
        /// <param name="allowedCategories"></param>
        /// <returns></returns>
        public List<Category> GetCategoryParents(Category category, List<Category> allowedCategories)
        {
            var cacheKey = string.Concat(CacheKeys.Category.StartsWith, "GetCategoryParents-", allowedCategories.GetHashCode());
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                var path = category.Path;
                var cats = new List<Category>();
                if (!string.IsNullOrEmpty(path))
                {
                    var catGuids = path.Trim().Split(',').Select(x => new Guid(x)).ToList();
                    if (!catGuids.Contains(category.Id))
                    {
                        catGuids.Add(category.Id);
                    }
                    cats = Get(catGuids).ToList();
                }
                var allowedCatIds = new List<Guid>();
                if (allowedCategories.Any())
                {
                    allowedCatIds.AddRange(allowedCategories.Select(x => x.Id));
                }
                return cats.Where(x => allowedCatIds.Contains(x.Id)).ToList();
            });
        }

        /// <summary>
        /// Delete a category
        /// </summary>
        /// <param name="category"></param>
        public void Delete(Category category)
        {
            // Check if anyone else if using this role
            var okToDelete = !category.Topics.Any();

            if (okToDelete)
            {
                // Get any categorypermissionforoles and delete these first
                var rolesToDelete = _categoryPermissionForRoleService.GetByCategory(category.Id);

                foreach (var categoryPermissionForRole in rolesToDelete)
                {
                    _categoryPermissionForRoleService.Delete(categoryPermissionForRole);
                }

                var categoryNotificationsToDelete = new List<CategoryNotification>();
                categoryNotificationsToDelete.AddRange(category.CategoryNotifications);
                foreach (var categoryNotification in categoryNotificationsToDelete)
                {
                    _categoryNotificationService.Delete(categoryNotification);
                }

                _context.Category.Remove(category);
            }
            else
            {
                var inUseBy = new List<Entity>();
                inUseBy.AddRange(category.Topics);
                throw new InUseUnableToDeleteException(inUseBy);
            }
        }

        public Category GetBySlug(string slug)
        {
            slug = StringUtils.GetSafeHtml(slug);
            var cacheKey = string.Concat(CacheKeys.Category.StartsWith, "GetBySlug-", slug);
            return _cacheService.CachePerRequest(cacheKey, () => _context.Category.FirstOrDefault(x => x.Slug == slug));
        }

        public IList<Category> GetBySlugLike(string slug)
        {
            slug = StringUtils.GetSafeHtml(slug);
            var cacheKey = string.Concat(CacheKeys.Category.StartsWith, "GetBySlugLike-", slug);
            return _cacheService.CachePerRequest(cacheKey, () => _context.Category
                                                                    .Where(x => x.Slug.Contains(slug))
                                                                    .ToList());
        }

        /// <summary>
        /// Gets all categories right the way down
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public IList<Category> GetAllDeepSubCategories(Category category)
        {
            var cacheKey = string.Concat(CacheKeys.Category.StartsWith, "GetAllDeepSubCategories-", category.Id);
            return _cacheService.CachePerRequest(cacheKey, () =>
            {
                var catGuid = category.Id.ToString().ToLower();
                return _context.Category
                        .Where(x => x.Path != null && x.Path.ToLower().Contains(catGuid))
                        .OrderBy(x => x.SortOrder)
                        .ToList();
            });
        }
    }
}

