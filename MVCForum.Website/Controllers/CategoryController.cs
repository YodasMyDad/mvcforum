namespace MvcForum.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Application.CustomActionResults;
    using Core;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Core.Models.Entities;
    using Core.Models.Enums;
    using Core.Models.General;
    using ViewModels.Breadcrumb;
    using ViewModels.Category;
    using ViewModels.Mapping;

    public partial class CategoryController : BaseController
    {
        private readonly INotificationService _notificationService;
        private readonly ICategoryService _categoryService;
        private readonly IFavouriteService _favouriteService;
        private readonly IPollService _pollAnswerService;
        private readonly IPostService _postService;
        private readonly IRoleService _roleService;
        private readonly ITopicService _topicService;
        private readonly IVoteService _voteService;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="loggingService"> </param>
        /// <param name="membershipService"></param>
        /// <param name="localizationService"></param>
        /// <param name="roleService"></param>
        /// <param name="categoryService"></param>
        /// <param name="settingsService"> </param>
        /// <param name="topicService"> </param>
        /// <param name="cacheService"></param>
        /// <param name="postService"></param>
        /// <param name="pollService"></param>
        /// <param name="voteService"></param>
        /// <param name="favouriteService"></param>
        /// <param name="context"></param>
        /// <param name="notificationService"></param>
        public CategoryController(ILoggingService loggingService, IMembershipService membershipService,
            ILocalizationService localizationService, IRoleService roleService, ICategoryService categoryService,
            ISettingsService settingsService, ITopicService topicService,
            ICacheService cacheService,
            IPostService postService,
            IPollService pollService, IVoteService voteService, IFavouriteService favouriteService,
            IMvcForumContext context, INotificationService notificationService)
            : base(loggingService, membershipService, localizationService, roleService,
                settingsService, cacheService, context)
        {
            _categoryService = categoryService;
            _topicService = topicService;
            _pollAnswerService = pollService;
            _voteService = voteService;
            _favouriteService = favouriteService;
            _notificationService = notificationService;
            _postService = postService;
            _roleService = roleService;
        }

        public ActionResult Index()
        {
            return View();
        }

        [ChildActionOnly]
        public virtual PartialViewResult ListMainCategories()
        {
            // TODO - OutputCache and add clear to post/topic/category delete/create/edit

            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);
            var catViewModel = new CategoryListSummaryViewModel
            {
                AllPermissionSets =
                    ViewModelMapping.GetPermissionsForCategories(_categoryService.GetAllMainCategoriesInSummary(),
                        _roleService, loggedOnUsersRole)
            };
            return PartialView(catViewModel);
        }


        [ChildActionOnly]
        public virtual PartialViewResult ListSections()
        {
            // TODO - How can we cache this effectively??
            // Get all sections, and include all Categories

            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

            // Model for the sections
            var allSections = new List<SectionListViewModel>();

            // Get sections from the DB
            var dbSections = _categoryService.GetAllSections();

            // Get all categories grouped by section
            var groupedCategories = _categoryService.GetAllMainCategoriesInSummaryGroupedBySection();

            // Loop sections
            foreach (var dbSection in dbSections)
            {
                var categoriesInSection = groupedCategories[dbSection.Id];
                var allPermissionSets = ViewModelMapping.GetPermissionsForCategories(categoriesInSection, _roleService, loggedOnUsersRole, true);

                allSections.Add(new SectionListViewModel
                {
                    Section = dbSection,
                    AllPermissionSets = allPermissionSets
                });

            }

            return PartialView(allSections);
        }

        [ChildActionOnly]
        public virtual PartialViewResult ListCategorySideMenu()
        {
            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);
            var catViewModel = new CategoryListViewModel
            {
                AllPermissionSets = ViewModelMapping.GetPermissionsForCategories(_categoryService.GetAll(), _roleService,
                        loggedOnUsersRole)
            };
            return PartialView(catViewModel);
        }

        [Authorize]
        [ChildActionOnly]
        public virtual PartialViewResult GetSubscribedCategories()
        {
            var viewModel = new List<CategoryViewModel>();

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


            return PartialView(viewModel);
        }


        [ChildActionOnly]
        public virtual PartialViewResult GetCategoryBreadcrumb(Category category)
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

        public virtual async Task<ActionResult> Show(string slug, int? p)
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

            if (!permissions[ForumConfiguration.Instance.PermissionDenyAccess].IsTicked)
            {
                var topics = await _topicService.GetPagedTopicsByCategory(pageIndex,
                    SettingsService.GetSettings().TopicsPerPage,
                    int.MaxValue, category.Category.Id);

                var topicViewModels = ViewModelMapping.CreateTopicViewModels(topics.ToList(), RoleService,
                    loggedOnUsersRole, loggedOnReadOnlyUser, allowedCategories, SettingsService.GetSettings(),
                    _postService, _notificationService, _pollAnswerService, _voteService, _favouriteService);

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
                    IsSubscribed = User.Identity.IsAuthenticated && _notificationService
                                       .GetCategoryNotificationsByUserAndCategory(loggedOnReadOnlyUser, category.Category).Any()
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

        [OutputCache(Duration = (int)CacheTimes.TwoHours)]
        public virtual ActionResult CategoryRss(string slug)
        {
            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

            // get an rss lit ready
            var rssTopics = new List<RssItem>();

            // Get the category
            var category = _categoryService.Get(slug);

            // check the user has permission to this category
            var permissions = RoleService.GetPermissions(category, loggedOnUsersRole);

            if (!permissions[ForumConfiguration.Instance.PermissionDenyAccess].IsTicked)
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