using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Exceptions;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Utilities;

namespace MVCForum.Services
{
    public partial class CategoryService : ICategoryService
    {
        private readonly IRoleService _roleService;
        private readonly ICategoryRepository _categoryRepository;
        private readonly ICategoryNotificationService _categoryNotificationService;
        private readonly ICategoryPermissionForRoleRepository _categoryPermissionForRoleRepository;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="categoryPermissionForRoleRepository"> </param>
        /// <param name="roleService"> </param>
        /// <param name="categoryRepository"> </param>
        /// <param name="categoryNotificationService"> </param>
        public CategoryService(ICategoryRepository categoryRepository, ICategoryPermissionForRoleRepository categoryPermissionForRoleRepository,
            IRoleService roleService, ICategoryNotificationService categoryNotificationService)
        {
            _categoryRepository = categoryRepository;
            _categoryPermissionForRoleRepository = categoryPermissionForRoleRepository;
            _roleService = roleService;
            _categoryNotificationService = categoryNotificationService;
        }

        /// <summary>
        /// Return all categories
        /// </summary>
        /// <returns></returns>
        public List<Category> GetAll()
        {
            // Cache per request for speed - As this is hit constantly for permissions
            if (HttpContext.Current != null)
            {
                const string key = "get-all-categories";
                if (!HttpContext.Current.Items.Contains(key))
                {
                    // These are now in order
                    var orderedCategories = new List<Category>();
                    var allCats = _categoryRepository.GetAll().ToList();
                    foreach (var parentCategory in allCats.Where(x => x.ParentCategory == null).OrderBy(x => x.SortOrder))
                    {
                        // Add the main category
                        parentCategory.Level = 1;
                        orderedCategories.Add(parentCategory);

                        // Add subcategories under this
                        orderedCategories.AddRange(GetSubCategories(parentCategory, allCats));
                    }

                    HttpContext.Current.Items.Add(key, orderedCategories);
                }
                return (List<Category>)HttpContext.Current.Items[key];
            }
            return _categoryRepository.GetAll().ToList();
        }

        public List<Category> GetSubCategories(Category category, List<Category> allCategories, int level = 2)
        {
            var catsToReturn = new List<Category>();
            var cats = allCategories.Where(x => x.ParentCategory != null && x.ParentCategory.Id == category.Id).OrderBy(x =>x.SortOrder);
            foreach (var cat in cats)
            {
                cat.Level = level;
                catsToReturn.Add(cat);
                catsToReturn.AddRange(GetSubCategories(cat, allCategories, level + 1));
            }

            return catsToReturn;
        }

        public List<SelectListItem> GetBaseSelectListCategories(List<Category> allowedCategories)
        {
            var cats = new List<SelectListItem> { new SelectListItem { Text = "", Value = "" } };
            foreach (var cat in allowedCategories)
            {
                var catName = string.Concat(LevelDashes(cat.Level), cat.Level > 1 ? " " : "", cat.Name);
                cats.Add(new SelectListItem { Text = catName, Value = cat.Id.ToString() });
            }
            return cats;
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
            return _categoryRepository.GetAllSubCategories(parentId);
        }

        /// <summary>
        /// Get all main categories (Categories with no parent category)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Category> GetAllMainCategories()
        {
            return _categoryRepository.GetMainCategories();
        }

        /// <summary>
        /// Return allowed categories based on the users role
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public List<Category> GetAllowedCategories(MembershipRole role)
        {
            return GetAllowedCategories(role, AppConstants.PermissionDenyAccess);
        }

        public List<Category> GetAllowedCategories(MembershipRole role, string actionType)
        {
            if (HttpContext.Current != null)
            {
                // Store per request
                var key = string.Concat("allowed-categories", role.Id, actionType);
                if (!HttpContext.Current.Items.Contains(key))
                {
                    HttpContext.Current.Items.Add(key, GetAllowedCategoriesCode(role, actionType));
                }
                return (List<Category>)HttpContext.Current.Items[key];
            }
            return GetAllowedCategoriesCode(role, actionType);
        }

        private List<Category> GetAllowedCategoriesCode(MembershipRole role, string actionType)
        {
            var filteredCats = new List<Category>();
            var allCats = GetAll();
            foreach (var category in allCats)
            {
                var permissionSet = _roleService.GetPermissions(category, role);
                if (!permissionSet[actionType].IsTicked)
                {
                    filteredCats.Add(category);
                }
            }
            return filteredCats;
        }

        /// <summary>
        /// Add a new category
        /// </summary>
        /// <param name="category"></param>
        public void Add(Category category)
        {
            // Sanitize
            category = SanitizeCategory(category);

            // Set the create date
            category.DateCreated = DateTime.UtcNow;

            // url slug generator
            category.Slug = ServiceHelpers.GenerateSlug(category.Name, _categoryRepository.GetBySlugLike(ServiceHelpers.CreateUrl(category.Name)), null);            

            // Add the category
            _categoryRepository.Add(category);
        }

        /// <summary>
        /// Keep slug in line with name
        /// </summary>
        /// <param name="category"></param>
        public void UpdateSlugFromName(Category category)
        {
            // Sanitize
            category = SanitizeCategory(category);

            category.Slug = ServiceHelpers.GenerateSlug(category.Name, _categoryRepository.GetBySlugLike(category.Slug), category.Slug);
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
            return _categoryRepository.Get(id);
        }

        public IList<Category> Get(IList<Guid> ids, bool fullGraph = false)
        {
            return _categoryRepository.Get(ids, fullGraph);
        }

        /// <summary>
        /// Return model with Sub categories
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public CategoryWithSubCategories GetBySlugWithSubCategories(string slug)
        {
            return _categoryRepository.GetBySlugWithSubCategories(slug);
        }

        /// <summary>
        /// Return category by Url slug
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public Category Get(string slug)
        {
            return _categoryRepository.GetBySlug(StringUtils.GetSafeHtml(slug));
        }

        public List<Category> GetCategoryParents(Category category, List<Category> allowedCategories)
        {
            var cats = _categoryRepository.GetCategoryParents(category);
            var allowedCatIds = new List<Guid>();
            if (allowedCategories != null && allowedCategories.Any())
            {
                allowedCatIds.AddRange(allowedCategories.Select(x => x.Id));
            }
            return cats.Where(x => allowedCatIds.Contains(x.Id)).ToList();
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
                var rolesToDelete = _categoryPermissionForRoleRepository.GetByCategory(category.Id);

                foreach (var categoryPermissionForRole in rolesToDelete)
                {
                    _categoryPermissionForRoleRepository.Delete(categoryPermissionForRole);
                }

                var categoryNotificationsToDelete = new List<CategoryNotification>();
                categoryNotificationsToDelete.AddRange(category.CategoryNotifications);
                foreach (var categoryNotification in categoryNotificationsToDelete)
                {
                    _categoryNotificationService.Delete(categoryNotification);
                }

                _categoryRepository.Delete(category);
            }
            else
            {
                var inUseBy = new List<Entity>();
                inUseBy.AddRange(category.Topics);
                throw new InUseUnableToDeleteException(inUseBy);
            }
        }



    }
}

