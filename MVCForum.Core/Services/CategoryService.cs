namespace MvcForum.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using Constants;
    using Interfaces;
    using Interfaces.Pipeline;
    using Interfaces.Services;
    using Models;
    using Models.Entities;
    using Models.General;
    using Pipeline;
    using Reflection;
    using Utilities;

    public partial class CategoryService : ICategoryService
    {
        private readonly ICacheService _cacheService;
        private readonly INotificationService _notificationService;
        private readonly ICategoryPermissionForRoleService _categoryPermissionForRoleService;
        private IMvcForumContext _context;
        private readonly IRoleService _roleService;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="roleService"> </param>
        /// <param name="notificationService"> </param>
        /// <param name="categoryPermissionForRoleService"></param>
        /// <param name="cacheService"></param>
        public CategoryService(IMvcForumContext context, IRoleService roleService,
            INotificationService notificationService,
            ICategoryPermissionForRoleService categoryPermissionForRoleService, ICacheService cacheService)
        {
            _roleService = roleService;
            _notificationService = notificationService;
            _categoryPermissionForRoleService = categoryPermissionForRoleService;
            _cacheService = cacheService;
            _context = context;
        }

        /// <inheritdoc />
        public void RefreshContext(IMvcForumContext context)
        {
            _context = context;
            _roleService.RefreshContext(context);
            _notificationService.RefreshContext(context);
            _categoryPermissionForRoleService.RefreshContext(context);
        }

        /// <inheritdoc />
        public async Task<int> SaveChanges()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        ///     Return all categories
        /// </summary>
        /// <returns></returns>
        public List<Category> GetAll()
        {
            var cachedCategories = _cacheService.Get<List<Category>>("CategoryList.GetAll");
            if (cachedCategories == null)
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
                cachedCategories = orderedCategories;
            }
            return cachedCategories;
        }

        public List<Category> GetSubCategories(Category category, List<Category> allCategories, int level = 2)
        {

                var catsToReturn = new List<Category>();
                var cats = allCategories.Where(x => x.ParentCategory != null && x.ParentCategory.Id == category.Id)
                    .OrderBy(x => x.SortOrder);
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
                var cats = new List<SelectListItem> {new SelectListItem {Text = "", Value = ""}};
                foreach (var cat in allowedCategories)
                {
                    var catName = string.Concat(LevelDashes(cat.Level), cat.Level > 1 ? " " : "", cat.Name);
                    cats.Add(new SelectListItem {Text = catName, Value = cat.Id.ToString()});
                }
                return cats;
        
        }

        /// <summary>
        ///     Return all sub categories from a parent category id
        /// </summary>
        /// <param name="parentId"></param>
        /// <returns></returns>
        public IEnumerable<Category> GetAllSubCategories(Guid parentId)
        {
            return _context.Category.AsNoTracking()
                .Where(x => x.ParentCategory.Id == parentId)
                .OrderBy(x => x.SortOrder);
        }

        /// <summary>
        ///     Get all main categories (Categories with no parent category)
        /// </summary>
        /// <returns></returns>
        public IEnumerable<Category> GetAllMainCategories()
        {
            return _context.Category.AsNoTracking()
                .Include(x => x.ParentCategory)
                .Where(cat => cat.ParentCategory == null)
                .OrderBy(x => x.SortOrder)
                .ToList();
        }

        /// <inheritdoc />
        public IEnumerable<CategorySummary> GetAllMainCategoriesInSummary()
        {
            return _context.Category.AsNoTracking()
                .Include(x => x.ParentCategory)
                .Include(x => x.Section)
                .Where(cat => cat.ParentCategory == null)
                .OrderBy(x => x.SortOrder)
                .Select(x => new CategorySummary
                {
                    Category = x,
                    TopicCount = x.Topics.Count,
                    PostCount = x.Topics.SelectMany(p => p.Posts).Count(), // TODO - Should this be a seperate call?
                    MostRecentTopic = x.Topics.OrderByDescending(t => t.LastPost.DateCreated).FirstOrDefault() // TODO - Should this be a seperate call?
                })
                .ToList();
        }

        /// <inheritdoc />
        public ILookup<Guid, CategorySummary> GetAllMainCategoriesInSummaryGroupedBySection()
        {
            return _context.Category.AsNoTracking()
                .Include(x => x.ParentCategory)
                .Include(x => x.Section)
                .Where(x => x.ParentCategory == null && x.Section != null)
                .OrderBy(x => x.SortOrder)
                .Select(x => new CategorySummary
                {
                    Category = x,
                    TopicCount = x.Topics.Count,
                    PostCount = x.Topics.SelectMany(p => p.Posts).Count(), // TODO - Should this be a seperate call?
                    MostRecentTopic = x.Topics.OrderByDescending(t => t.LastPost.DateCreated).FirstOrDefault() // TODO - Should this be a seperate call?
                })
                .ToList()
                .ToLookup(x => x.Category.Section.Id);
        }

        /// <summary>
        ///     Return allowed categories based on the users role
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public List<Category> GetAllowedCategories(MembershipRole role)
        {
            return GetAllowedCategories(role, ForumConfiguration.Instance.PermissionDenyAccess);
        }

        public List<Category> GetAllowedCategories(MembershipRole role, string actionType)
        {
            return GetAllowedCategoriesCode(role, actionType);
        }

        /// <summary>
        ///     Add a new category
        /// </summary>
        /// <param name="category"></param>
        /// <param name="postedFiles"></param>
        /// <param name="parentCategory"></param>
        /// <param name="section"></param>
        public async Task<IPipelineProcess<Category>> Create(Category category, HttpPostedFileBase[] postedFiles, Guid? parentCategory, Guid? section)
        {
            // Get the pipelines
            var categoryPipes = ForumConfiguration.Instance.PipelinesCategoryCreate;

            // The model to process
            var piplineModel = new PipelineProcess<Category>(category);

            // Add parent category
            if (parentCategory != null)
            {
                piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.ParentCategory, parentCategory);
            }

            // Add section
            if (section != null)
            {
                piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.Section, section);
            }

            // Add posted files
            if (postedFiles != null && postedFiles.Any(x => x != null))
            {
                piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.PostedFiles, postedFiles);
            }

            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.Username, HttpContext.Current.User.Identity.Name);

            // Get instance of the pipeline to use
            var pipeline = new Pipeline<IPipelineProcess<Category>, Category>(_context);

            // Register the pipes 
            var allCategoryPipes = ImplementationManager.GetInstances<IPipe<IPipelineProcess<Category>>>();

            // Loop through the pipes and add the ones we want
            foreach (var pipe in categoryPipes)
            {
                if (allCategoryPipes.ContainsKey(pipe))
                {
                    pipeline.Register(allCategoryPipes[pipe]);
                }
            }

            return await pipeline.Process(piplineModel);
        }

        /// <inheritdoc />
        public async Task<IPipelineProcess<Category>> Edit(Category category, HttpPostedFileBase[] postedFiles, Guid? parentCategory, Guid? section)
        {
            // Get the pipelines
            var categoryPipes = ForumConfiguration.Instance.PipelinesCategoryUpdate;

            // The model to process
            var piplineModel = new PipelineProcess<Category>(category);

            // Add parent category
            if (parentCategory != null)
            {
                piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.ParentCategory, parentCategory);
            }

            // Add section
            if (section != null)
            {
                piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.Section, section);
            }

            // Add posted files
            if (postedFiles != null && postedFiles.Any(x => x != null))
            {
                piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.PostedFiles, postedFiles);
            }

            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.Username, HttpContext.Current.User.Identity.Name);
            piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.IsEdit, true);

            // Get instance of the pipeline to use
            var pipeline = new Pipeline<IPipelineProcess<Category>, Category>(_context);

            // Register the pipes 
            var allCategoryPipes = ImplementationManager.GetInstances<IPipe<IPipelineProcess<Category>>>();

            // Loop through the pipes and add the ones we want
            foreach (var pipe in categoryPipes)
            {
                if (allCategoryPipes.ContainsKey(pipe))
                {
                    pipeline.Register(allCategoryPipes[pipe]);
                }
            }

            return await pipeline.Process(piplineModel);
        }

        /// <summary>
        ///     Keep slug in line with name
        /// </summary>
        /// <param name="category"></param>
        public void UpdateSlugFromName(Category category)
        {
            // Sanitize
            category = SanitizeCategory(category);

            var updateSlug = true;

            // Check if slug has changed as this could be an update
            if (!string.IsNullOrWhiteSpace(category.Slug))
            {
                var categoryBySlug = GetBySlugWithSubCategories(category.Slug);
                if (categoryBySlug.Category.Id == category.Id)
                {
                    updateSlug = false;
                }
            }

            if (updateSlug)
            {
                category.Slug = ServiceHelpers.GenerateSlug(category.Name, GetBySlugLike(category.Slug).Select(x => x.Slug).ToList(), category.Slug);
            }
        }

        /// <summary>
        ///     Sanitizes a category
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
        ///     Return category by id
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Category Get(Guid id)
        {
            return _context.Category.FirstOrDefault(x => x.Id == id);
        }

        public IList<Category> Get(IList<Guid> ids, bool fullGraph = false)
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
        }

        /// <summary>
        ///     Return model with Sub categories
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public CategoryWithSubCategories GetBySlugWithSubCategories(string slug)
        {
            slug = StringUtils.SafePlainText(slug);

            var cat = (from category in _context.Category.AsNoTracking()
                where category.Slug == slug
                select new CategoryWithSubCategories
                {
                    Category = category,
                    SubCategories = from cats in _context.Category
                        where cats.ParentCategory.Id == category.Id
                        select cats
                }).FirstOrDefault();

            return cat;
        }

        /// <summary>
        ///     Return category by Url slug
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public Category Get(string slug)
        {
            return GetBySlug(slug);
        }

        /// <summary>
        ///     Gets the category parents
        /// </summary>
        /// <param name="category"></param>
        /// <param name="allowedCategories"></param>
        /// <returns></returns>
        public List<Category> GetCategoryParents(Category category, List<Category> allowedCategories)
        {
            var path = category.Path;
            var cats = new List<Category>();
            if (!string.IsNullOrWhiteSpace(path))
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
        }

        /// <summary>
        ///     Delete a category
        /// </summary>
        /// <param name="category"></param>
        public async Task<IPipelineProcess<Category>> Delete(Category category)
        {
            // Get the pipelines
            var categoryPipes = ForumConfiguration.Instance.PipelinesCategoryDelete;

            // The model to process
            var piplineModel = new PipelineProcess<Category>(category);

            // Get instance of the pipeline to use
            var pipeline = new Pipeline<IPipelineProcess<Category>, Category>(_context);

            // Register the pipes 
            var allCategoryPipes = ImplementationManager.GetInstances<IPipe<IPipelineProcess<Category>>>();

            // Loop through the pipes and add the ones we want
            foreach (var pipe in categoryPipes)
            {
                if (allCategoryPipes.ContainsKey(pipe))
                {
                    pipeline.Register(allCategoryPipes[pipe]);
                }
            }

            return await pipeline.Process(piplineModel);
        }

        public Category GetBySlug(string slug)
        {
            slug = StringUtils.GetSafeHtml(slug);
            return _context.Category.AsNoTracking().FirstOrDefault(x => x.Slug == slug);
        }

        public IList<Category> GetBySlugLike(string slug)
        {
            slug = StringUtils.GetSafeHtml(slug);

            return _context.Category.AsNoTracking()
                .Where(x => x.Slug.Contains(slug))
                .ToList();
        }

        /// <summary>
        ///     Gets all categories right the way down
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public IList<Category> GetAllDeepSubCategories(Category category)
        {
            var catGuid = category.Id.ToString().ToLower();
            return _context.Category
                .Where(x => x.Path != null && x.Path.ToLower().Contains(catGuid))
                .OrderBy(x => x.SortOrder)
                .ToList();
        }

        /// <inheritdoc />
        public void SortPath(Category category, Category parentCategory)
        {
            // Append the path from the parent category
            var path = !string.IsNullOrWhiteSpace(parentCategory.Path) ? 
                string.Concat(parentCategory.Path, ",", parentCategory.Id.ToString()) : 
                parentCategory.Id.ToString();

            category.Path = path;
        }

        /// <inheritdoc />
        public List<Section> GetAllSections()
        {
            return _context.Section.AsNoTracking().Include(x => x.Categories).OrderBy(x => x.SortOrder).ToList();
        }

        /// <inheritdoc />
        public Section GetSection(Guid id)
        {
            return _context.Section.Find(id);
        }

        /// <inheritdoc />
        public void DeleteSection(Guid id)
        {
            var section = _context.Section.Find(id);
            if (section != null)
            {
                _context.Section.Remove(section);
                _context.SaveChanges();
            }
        }

        private static string LevelDashes(int level)
        {
            if (level > 1)
            {
                var sb = new StringBuilder();
                for (var i = 0; i < level - 1; i++)
                {
                    sb.Append("-");
                }
                return sb.ToString();
            }
            return string.Empty;
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
                    // Only add it category is NOT locked
                    filteredCats.Add(category);
                }
            }
            return filteredCats;
        }
    }
}