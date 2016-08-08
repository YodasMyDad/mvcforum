namespace MVCForum.Website.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using Application;
    using Domain.Constants;
    using Domain.DomainModel;
    using Domain.DomainModel.Enums;
    using Domain.Interfaces.Services;
    using Domain.Interfaces.UnitOfWork;
    using ViewModels;
    using ViewModels.Mapping;

    public partial class CategoryController : BaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly ICategoryNotificationService _categoryNotificationService;
        private readonly ITopicService _topicService;
        private readonly IRoleService _roleService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="loggingService"> </param>
        /// <param name="unitOfWorkManager"> </param>
        /// <param name="membershipService"></param>
        /// <param name="localizationService"></param>
        /// <param name="roleService"></param>
        /// <param name="categoryService"></param>
        /// <param name="settingsService"> </param>
        /// <param name="topicService"> </param>
        /// <param name="categoryNotificationService"> </param>
        /// <param name="cacheService"></param>
        public CategoryController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager,
            IMembershipService membershipService,
            ILocalizationService localizationService,
            IRoleService roleService,
            ICategoryService categoryService,
            ISettingsService settingsService, ITopicService topicService, ICategoryNotificationService categoryNotificationService, ICacheService cacheService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService, cacheService)
        {
            _categoryService = categoryService;
            _topicService = topicService;
            _categoryNotificationService = categoryNotificationService;
            _roleService = roleService;
        }

        public ActionResult Index()
        {
            return View();
        }

        [ChildActionOnly]
        public PartialViewResult ListMainCategories()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {

                var catViewModel = new CategoryListViewModel
                {
                    AllPermissionSets = ViewModelMapping.GetPermissionsForCategories(_categoryService.GetAllMainCategories(), _roleService, UsersRole)
                };
                return PartialView(catViewModel);
            }
        }

        [ChildActionOnly]
        public PartialViewResult ListCategorySideMenu()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var catViewModel = new CategoryListViewModel
                {
                    AllPermissionSets = ViewModelMapping.GetPermissionsForCategories(_categoryService.GetAll(), _roleService, UsersRole)
                };
                return PartialView(catViewModel);
            }
        }

        [Authorize]
        [ChildActionOnly]
        public PartialViewResult GetSubscribedCategories()
        {
            var viewModel = new List<CategoryViewModel>();
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var categories = LoggedOnReadOnlyUser.CategoryNotifications.Select(x => x.Category);
                foreach (var category in categories)
                {
                    var permissionSet = RoleService.GetPermissions(category, UsersRole);
                    var topicCount = category.Topics.Count;
                    var latestTopicInCategory = category.Topics.OrderByDescending(x => x.LastPost.DateCreated).FirstOrDefault();
                    var postCount = (category.Topics.SelectMany(x => x.Posts).Count() - 1);
                    var model = new CategoryViewModel
                    {
                        Category = category,
                        LatestTopic = latestTopicInCategory,
                        Permissions = permissionSet,
                        PostCount = postCount,
                        TopicCount = topicCount,
                        ShowUnSubscribedLink = true
                    };
                    viewModel.Add(model);
                }
            }

            return PartialView(viewModel);
        }


        [ChildActionOnly]
        public PartialViewResult GetCategoryBreadcrumb(Category category)
        {
            var allowedCategories = _categoryService.GetAllowedCategories(UsersRole);

            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var viewModel = new BreadcrumbViewModel
                {
                    Categories = _categoryService.GetCategoryParents(category,allowedCategories),
                    Category = category
                };
                return PartialView("GetCategoryBreadcrumb", viewModel);
            }
        }

        public ActionResult Show(string slug, int? p)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                // Get the category
                var category = _categoryService.GetBySlugWithSubCategories(slug);

                // Allowed Categories for this user
                var allowedCategories = _categoryService.GetAllowedCategories(UsersRole);

                // Set the page index
                var pageIndex = p ?? 1;

                // check the user has permission to this category
                var permissions = RoleService.GetPermissions(category.Category, UsersRole);

                if (!permissions[SiteConstants.Instance.PermissionDenyAccess].IsTicked)
                {

                    var topics = _topicService.GetPagedTopicsByCategory(pageIndex,
                                                                        SettingsService.GetSettings().TopicsPerPage,
                                                                        int.MaxValue, category.Category.Id);

                    var topicViewModels = ViewModelMapping.CreateTopicViewModels(topics.ToList(), RoleService, UsersRole, LoggedOnReadOnlyUser, allowedCategories, SettingsService.GetSettings());

                    // Create the main view model for the category
                    var viewModel = new CategoryViewModel
                        {
                            Permissions = permissions,
                            Topics = topicViewModels,
                            Category = category.Category,
                            PageIndex = pageIndex,
                            TotalCount = topics.TotalCount,
                            TotalPages = topics.TotalPages,
                            User = LoggedOnReadOnlyUser,
                            IsSubscribed = UserIsAuthenticated && (_categoryNotificationService.GetByUserAndCategory(LoggedOnReadOnlyUser, category.Category).Any())
                        };

                    // If there are subcategories then add then with their permissions
                    if (category.SubCategories.Any())
                    {
                        var subCatViewModel = new CategoryListViewModel
                            {
                                AllPermissionSets = new Dictionary<Category, PermissionSet>()
                            };
                        foreach (var subCategory in category.SubCategories)
                        {
                            var permissionSet = RoleService.GetPermissions(subCategory, UsersRole);
                            subCatViewModel.AllPermissionSets.Add(subCategory, permissionSet);
                        }
                        viewModel.SubCategories = subCatViewModel;
                    }

                    return View(viewModel);
                }

                return ErrorToHomePage(LocalizationService.GetResourceString("Errors.NoPermission"));
            }
        }

        [OutputCache(Duration = (int)CacheTimes.TwoHours)]
        public ActionResult CategoryRss(string slug)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {


                // get an rss lit ready
                var rssTopics = new List<RssItem>();

                // Get the category
                var category = _categoryService.Get(slug);

                // check the user has permission to this category
                var permissions = RoleService.GetPermissions(category, UsersRole);

                if (!permissions[SiteConstants.Instance.PermissionDenyAccess].IsTicked)
                {
                    var topics = _topicService.GetRssTopicsByCategory(50, category.Id);

                    rssTopics.AddRange(topics.Select(x =>
                                                         {
                                                             var firstOrDefault =
                                                                 x.Posts.FirstOrDefault(s => s.IsTopicStarter);
                                                             return firstOrDefault != null
                                                                        ? new RssItem
                                                                              {
                                                                                  Description = firstOrDefault.PostContent,
                                                                                  Link = x.NiceUrl,
                                                                                  Title = x.Name,
                                                                                  PublishedDate = x.CreateDate
                                                                              }
                                                                        : null;
                                                         }
                                           ));

                    return new RssResult(rssTopics, string.Format(LocalizationService.GetResourceString("Rss.Category.Title"), category.Name),
                                         string.Format(LocalizationService.GetResourceString("Rss.Category.Description"), category.Name));
                }

                return ErrorToHomePage(LocalizationService.GetResourceString("Errors.NothingToDisplay"));
            }
        }
    }
}
