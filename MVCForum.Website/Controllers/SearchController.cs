namespace MvcForum.Web.Controllers
{
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Core;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using ViewModels.Mapping;
    using ViewModels.Search;

    public partial class SearchController : BaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly IFavouriteService _favouriteService;
        private readonly IPostService _postService;
        private readonly IVoteService _voteService;

        public SearchController(ILoggingService loggingService, IMembershipService membershipService,
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService,
            IPostService postService, IVoteService voteService, IFavouriteService favouriteService,
            ICategoryService categoryService, ICacheService cacheService, IMvcForumContext context)
            : base(loggingService, membershipService, localizationService, roleService,
                settingsService, cacheService, context)
        {
            _postService = postService;
            _voteService = voteService;
            _favouriteService = favouriteService;
            _categoryService = categoryService;
        }

        [HttpGet]
        public virtual async Task<ActionResult> Index(int? p, string term)
        {
            if (!string.IsNullOrWhiteSpace(term))
            {
                if (!string.IsNullOrWhiteSpace(term))
                {
                    term = term.Trim();
                }

                var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
                var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

                // Get the global settings
                var settings = SettingsService.GetSettings();

                // Get allowed categories
                var allowedCategories = _categoryService.GetAllowedCategories(loggedOnUsersRole);


                // Set the page index
                var pageIndex = p ?? 1;

                // Get all the topics based on the search value
                var posts = await _postService.SearchPosts(pageIndex,
                    ForumConfiguration.Instance.SearchListSize,
                    int.MaxValue,
                    term,
                    allowedCategories);

                // Get all the permissions for these topics
                var topicPermissions =
                    ViewModelMapping.GetPermissionsForTopics(posts.Select(x => x.Topic), RoleService,
                        loggedOnUsersRole);

                // Get the post Ids
                var postIds = posts.Select(x => x.Id).ToList();

                // Get all votes for these posts
                var votes = _voteService.GetVotesByPosts(postIds);

                // Get all favourites for these posts
                var favs = _favouriteService.GetAllPostFavourites(postIds);

                // Create the post view models
                var viewModels = ViewModelMapping.CreatePostViewModels(posts.ToList(), votes, topicPermissions,
                    loggedOnReadOnlyUser, settings, favs);

                // create the view model
                var viewModel = new SearchViewModel
                {
                    Posts = viewModels,
                    PageIndex = pageIndex,
                    TotalCount = posts.TotalCount,
                    TotalPages = posts.TotalPages,
                    Term = term
                };

                return View(viewModel);
            }

            return RedirectToAction("Index", "Home");
        }


        [ChildActionOnly]
        public virtual PartialViewResult SideSearch()
        {
            return PartialView();
        }
    }
}