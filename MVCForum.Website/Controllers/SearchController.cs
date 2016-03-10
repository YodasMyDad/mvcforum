﻿using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Utilities;
using MVCForum.Website.Application;
using MVCForum.Website.ViewModels;
using MVCForum.Website.ViewModels.Mapping;

namespace MVCForum.Website.Controllers
{
    public partial class SearchController : BaseController
    {
        private readonly IPostService _postService;
        private readonly ITopicService _topicsService;
        private readonly ICategoryService _categoryService;
        private readonly IVoteService _voteService;
        private readonly IFavouriteService _favouriteService;

        public SearchController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager,
            IMembershipService membershipService, ILocalizationService localizationService,
            IRoleService roleService, ISettingsService settingsService,
            IPostService postService, ITopicService topicService, IVoteService voteService, IFavouriteService favouriteService, ICategoryService categoryService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _postService = postService;
            _topicsService = topicService;
            _voteService = voteService;
            _favouriteService = favouriteService;
            _categoryService = categoryService;
        }

        [HttpGet]
        public ActionResult Index(int? p, string term)
        {
            if (!string.IsNullOrEmpty(term))
            {
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    if (!string.IsNullOrEmpty(term))
                    {
                        term = term.Trim();
                    }

                    // Get the global settings
                    var settings = SettingsService.GetSettings();

                    // Get allowed categories
                    var allowedCategories = _categoryService.GetAllowedCategories(UsersRole);


                    // Set the page index
                    var pageIndex = p ?? 1;

                    // Get all the topics based on the search value
                    var posts = _postService.SearchPosts(pageIndex,
                                                         SiteConstants.Instance.SearchListSize,
                                                         int.MaxValue,
                                                         term,
                                                         allowedCategories);

                    // Get all the permissions for these topics
                    var topicPermissions = ViewModelMapping.GetPermissionsForTopics(posts.Select(x => x.Topic), RoleService, UsersRole);

                    // Get the post Ids
                    var postIds = posts.Select(x => x.Id).ToList();

                    // Get all votes for these posts
                    var votes = _voteService.GetVotesByPosts(postIds);

                    // Get all favourites for these posts
                    var favs = _favouriteService.GetAllPostFavourites(postIds);

                    // Create the post view models
                    var viewModels = ViewModelMapping.CreatePostViewModels(posts.ToList(), votes, topicPermissions, LoggedOnReadOnlyUser, settings, favs);

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
