namespace MvcForum.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Application.CustomActionResults;
    using Core.Constants;
    using Core.DomainModel.Entities;
    using Core.DomainModel.Enums;
    using Core.DomainModel.General;
    using Core.ExtensionMethods;
    using Core.Interfaces.Services;
    using Core.Interfaces.UnitOfWork;
    using ViewModels;
    using ViewModels.Mapping;

    public partial class CategoryController : BaseController
    {
        private readonly ICategoryNotificationService _categoryNotificationService;
        private readonly ICategoryService _categoryService;
        private readonly IRoleService _roleService;
        private readonly ITopicService _topicService;
        private readonly IPostService _postService;
        private readonly ITopicNotificationService _topicNotificationService;
        private readonly IPollAnswerService _pollAnswerService;
        private readonly IVoteService _voteService;
        private readonly IFavouriteService _favouriteService;

        /// <summary>
        ///     Constructor
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
        /// <param name="postService"></param>
        /// <param name="topicNotificationService"></param>
        /// <param name="pollAnswerService"></param>
        public CategoryController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager,
            IMembershipService membershipService,
            ILocalizationService localizationService,
            IRoleService roleService,
            ICategoryService categoryService,
            ISettingsService settingsService, ITopicService topicService,
            ICategoryNotificationService categoryNotificationService, ICacheService cacheService, IPostService postService, ITopicNotificationService topicNotificationService, IPollAnswerService pollAnswerService, IVoteService voteService, IFavouriteService favouriteService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService,
                settingsService, cacheService)
        {
            _categoryService = categoryService;
            _topicService = topicService;
            _categoryNotificationService = categoryNotificationService;
            _topicNotificationService = topicNotificationService;
            _pollAnswerService = pollAnswerService;
            _voteService = voteService;
            _favouriteService = favouriteService;
            _postService = postService;
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
                var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
                var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);
                var catViewModel = new CategoryListViewModel
                {
                    AllPermissionSets =
                        ViewModelMapping.GetPermissionsForCategories(_categoryService.GetAllMainCategories(),
                            _roleService, loggedOnUsersRole)
                };
                return PartialView(catViewModel);
            }
        }

        [ChildActionOnly]
        public PartialViewResult ListCategorySideMenu()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
                var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);
                var catViewModel = new CategoryListViewModel
                {
                    AllPermissionSets =
                        ViewModelMapping.GetPermissionsForCategories(_categoryService.GetAll(), _roleService, loggedOnUsersRole)
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
                var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
                var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);
                var categories = loggedOnReadOnlyUser.CategoryNotifications.Select(x => x.Category);
                foreach (var category in categories)
                {
                    var permissionSet = RoleService.GetPermissions(category, loggedOnUsersRole);
                    var topicCount = category.Topics.Count;
                    var latestTopicInCategory =
                        category.Topics.OrderByDescending(x => x.LastPost.DateCreated).FirstOrDefault();
                    var postCount = category.Topics.SelectMany(x => x.Posts).Count() - 1;
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
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
                var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);
                var allowedCategories = _categoryService.GetAllowedCategories(loggedOnUsersRole);
                var viewModel = new BreadcrumbViewModel
                {
                    Categories = _categoryService.GetCategoryParents(category, allowedCategories),
                    Category = category
                };
                return PartialView("GetCategoryBreadcrumb", viewModel);
            }
        }

        public async Task<ActionResult> Show(string slug, int? p)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
                var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

                // Get the category
                var category = _categoryService.GetBySlugWithSubCategories(slug);

                // Allowed Categories for this user
                var allowedCategories = _categoryService.GetAllowedCategories(loggedOnUsersRole);

                // Set the page index
                var pageIndex = p ?? 1;

                // check the user has permission to this category
                var permissions = RoleService.GetPermissions(category.Category, loggedOnUsersRole);

                if (!permissions[SiteConstants.Instance.PermissionDenyAccess].IsTicked)
                {
                    var topics = await _topicService.GetPagedTopicsByCategory(pageIndex,
                        SettingsService.GetSettings().TopicsPerPage,
                        int.MaxValue, category.Category.Id);

                    var topicViewModels = ViewModelMapping.CreateTopicViewModels(topics.ToList(), RoleService,
                        loggedOnUsersRole, loggedOnReadOnlyUser, allowedCategories, SettingsService.GetSettings(), _postService, _topicNotificationService, _pollAnswerService, _voteService, _favouriteService);

                    // Create the main view model for the category
                    var viewModel = new CategoryViewModel
                    {
                        Permissions = permissions,
                        Topics = topicViewModels,
                        Category = category.Category,
                        PageIndex = pageIndex,
                        TotalCount = topics.TotalCount,
                        TotalPages = topics.TotalPages,
                        User = loggedOnReadOnlyUser,
                        IsSubscribed = User.Identity.IsAuthenticated && _categoryNotificationService
                                           .GetByUserAndCategory(loggedOnReadOnlyUser, category.Category).Any()
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
                            var permissionSet = RoleService.GetPermissions(subCategory, loggedOnUsersRole);
                            subCatViewModel.AllPermissionSets.Add(subCategory, permissionSet);
                        }
                        viewModel.SubCategories = subCatViewModel;
                    }

                    return View(viewModel);
                }

                return ErrorToHomePage(LocalizationService.GetResourceString("Errors.NoPermission"));
            }
        }

        [OutputCache(Duration = (int) CacheTimes.TwoHours)]
        public ActionResult CategoryRss(string slug)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
                var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

                // get an rss lit ready
                var rssTopics = new List<RssItem>();

                // Get the category
                var category = _categoryService.Get(slug);

                // check the user has permission to this category
                var permissions = RoleService.GetPermissions(category, loggedOnUsersRole);

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

                    return new RssResult(rssTopics,
                        string.Format(LocalizationService.GetResourceString("Rss.Category.Title"), category.Name),
                        string.Format(LocalizationService.GetResourceString("Rss.Category.Description"),
                            category.Name));
                }

                return ErrorToHomePage(LocalizationService.GetResourceString("Errors.NothingToDisplay"));
            }
        }
    }
}