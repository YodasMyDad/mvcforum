using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Utilities;
using MVCForum.Website.ViewModels;

namespace MVCForum.Website.Controllers
{
    public class SearchController : BaseController
    {
        private readonly IPostService _postService;
        private readonly ITopicService _topicsService;
        private readonly ILuceneService _luceneService;

        private MembershipUser LoggedOnUser;
        private MembershipRole UsersRole;

        public SearchController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, 
            IMembershipService membershipService, ILocalizationService localizationService, 
            IRoleService roleService, ISettingsService settingsService, 
            IPostService postService, ITopicService topicService, ILuceneService luceneService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _postService = postService;
            _topicsService = topicService;
            _luceneService = luceneService;

            LoggedOnUser = UserIsAuthenticated ? MembershipService.GetUser(Username) : null;
            UsersRole = LoggedOnUser == null ? RoleService.GetRole(AppConstants.GuestRoleName) : LoggedOnUser.Roles.FirstOrDefault();
        }

        [HttpGet]
        public ActionResult Index(int? p, string term)
        {
            if (!string.IsNullOrEmpty(term))
            {
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    // Set the page index
                    var pageIndex = p ?? 1;

                    // Returns the formatted string to search on
                    var formattedSearchTerm = StringUtils.ReturnSearchString(term);

                    // Create an empty viewmodel
                    var viewModel = new SearchViewModel
                    {
                        Topics = new PagedList<Topic>(new List<Topic>(), 1, 20, 0),
                        AllPermissionSets = new Dictionary<Category, PermissionSet>(),
                        PageIndex = pageIndex,
                        TotalCount = 0,
                        Term = term
                    };

                    // Get lucene to search
                    var luceneResults = _luceneService.Search(formattedSearchTerm, pageIndex, SettingsService.GetSettings().TopicsPerPage);

                    // if there are no results from the filter return an empty search view model.
	                if (string.IsNullOrWhiteSpace(formattedSearchTerm))
	                {
                        return View(viewModel);
	                }

                    //// Get all the topics based on the search value
                    //var topics = _topicsService.SearchTopics(pageIndex,
                    //                                     SettingsService.GetSettings().TopicsPerPage,
                    //                                     AppConstants.ActiveTopicsListSize,
                    //                                     term);

                    var topics = _topicsService.GetTopicsByCsv(pageIndex, SettingsService.GetSettings().TopicsPerPage, AppConstants.ActiveTopicsListSize, luceneResults.Select(x => x.TopicId).ToList());

                    // Get all the categories for this topic collection
                    var categories = topics.Select(x => x.Category).Distinct();

                    // create the view model
                    viewModel = new SearchViewModel
                    {
                        Topics = topics,
                        AllPermissionSets = new Dictionary<Category, PermissionSet>(),
                        PageIndex = pageIndex,
                        TotalCount = luceneResults.TotalCount,
                        Term = formattedSearchTerm
                    };

                    // loop through the categories and get the permissions
                    foreach (var category in categories)
                    {
                        var permissionSet = RoleService.GetPermissions(category, UsersRole);
                        viewModel.AllPermissionSets.Add(category, permissionSet);
                    }

                    return View(viewModel);
                } 
            }

            return RedirectToAction("Index", "Home");
        }


        [ChildActionOnly]
        public PartialViewResult SideSearch()
        {
            return PartialView();
        }

    }
}
