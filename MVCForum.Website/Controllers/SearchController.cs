using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Utilities;
using MVCForum.Website.ViewModels;
using MVCForum.Website.ViewModels.Mapping;

namespace MVCForum.Website.Controllers
{
    public partial class SearchController : BaseController
    {
        private readonly IPostService _postService;
        private readonly ITopicService _topicsService;

        private MembershipUser LoggedOnUser;
        private MembershipRole UsersRole;

        public SearchController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, 
            IMembershipService membershipService, ILocalizationService localizationService, 
            IRoleService roleService, ISettingsService settingsService, 
            IPostService postService, ITopicService topicService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _postService = postService;
            _topicsService = topicService;

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
                    var settings = SettingsService.GetSettings();

                    // Set the page index
                    var pageIndex = p ?? 1;

                    // Get all the topics based on the search value
                    var topics = _topicsService.SearchTopics(pageIndex,
                                                         settings.TopicsPerPage,
                                                         SiteConstants.ActiveTopicsListSize,
                                                         term);

                    // Get the Topic View Models
                    var topicViewModels = ViewModelMapping.CreateTopicViewModels(topics.ToList(), RoleService, UsersRole, LoggedOnUser, settings);

                    // create the view model
                    var viewModel = new SearchViewModel
                    {
                        Topics = topicViewModels,
                        PageIndex = pageIndex,
                        TotalCount = topics.TotalCount,
                        TotalPages = topics.TotalPages,
                        Term = term
                    };

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
