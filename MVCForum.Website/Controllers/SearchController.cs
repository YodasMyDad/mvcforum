using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.ViewModels;

namespace MVCForum.Website.Controllers
{
    public class SearchController : BaseController
    {
        private readonly IPostService _postService;
        private readonly ITopicService _topicsService;

        public SearchController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, 
            IMembershipService membershipService, ILocalizationService localizationService, 
            IRoleService roleService, ISettingsService settingsService, 
            IPostService postService, ITopicService topicService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _postService = postService;
            _topicsService = topicService;
        }

        public ActionResult Index(int? p, string term)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                // Set the page index
                var pageIndex = p ?? 1;

                // Get all the topics based on the search value
                var topics = _topicsService.SearchTopics(pageIndex,
                                                     SettingsService.GetSettings().TopicsPerPage,
                                                     AppConstants.ActiveTopicsListSize,
                                                     term);


                // Get all the categories for this topic collection
                var categories = topics.Select(x => x.Category).Distinct();

                // create the view model
                var viewModel = new SearchViewModel
                {
                    Topics = topics,
                    AllPermissionSets = new Dictionary<Category, PermissionSet>(),
                    PageIndex = pageIndex,
                    TotalCount = topics.TotalCount
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

        [ChildActionOnly]
        public PartialViewResult SideSearch()
        {
            return PartialView();
        }

    }
}
