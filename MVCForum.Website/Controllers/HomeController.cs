using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Activity;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Utilities;
using MVCForum.Website.Application;
using MVCForum.Website.ViewModels;
using RssItem = MVCForum.Domain.DomainModel.RssItem;

namespace MVCForum.Website.Controllers
{
    public class HomeController : BaseController
    {
        private readonly ITopicService _topicService;
        private readonly IActivityService _activityService;

        private MembershipUser LoggedOnUser;
        private MembershipRole UsersRole;

        public HomeController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IActivityService activityService, IMembershipService membershipService,
            ITopicService topicService, ILocalizationService localizationService, IRoleService roleService,
            ISettingsService settingsService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _topicService = topicService;
            _activityService = activityService;

            LoggedOnUser = UserIsAuthenticated ? MembershipService.GetUser(Username) : null;
            UsersRole = LoggedOnUser == null ? RoleService.GetRole(AppConstants.GuestRoleName) : LoggedOnUser.Roles.FirstOrDefault();
        }

        //[OutputCache(Duration = 10, VaryByParam = "p")]
        public ActionResult Index(int? p)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                // Set the page index
                var pageIndex = p ?? 1;

                // Get the topics
                var topics = _topicService.GetRecentTopics(pageIndex,
                                                           SettingsService.GetSettings().TopicsPerPage,
                                                           AppConstants.ActiveTopicsListSize);

                // Get all the categories for this topic collection
                var categories = topics.Select(x => x.Category).Distinct();

                // create the view model
                var viewModel = new ActiveTopicsViewModel
                {
                    Topics = topics,
                    AllPermissionSets = new Dictionary<Category, PermissionSet>(),
                    PageIndex = pageIndex,
                    TotalCount = topics.TotalCount,
                    User = LoggedOnUser
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

        public ActionResult Leaderboard()
        {
            return View();
        }

        public ActionResult Activity(int? p)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                // Set the page index
                var pageIndex = p ?? 1;

                // Get the topics
                var activities = _activityService.GetPagedGroupedActivities(pageIndex, SettingsService.GetSettings().ActivitiesPerPage);

                // create the view model
                var viewModel = new AllRecentActivitiesViewModel
                {
                    Activities = activities,
                    PageIndex = pageIndex,
                    TotalCount = activities.TotalCount,
                };

                return View(viewModel);
            }
        }

        [OutputCache(Duration = AppConstants.DefaultCacheLengthInSeconds)]
        public ActionResult LatestRss()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                // get an rss lit ready
                var rssTopics = new List<RssItem>();

                // Get the latest topics
                var topics = _topicService.GetRecentRssTopics(AppConstants.ActiveTopicsListSize);

                // Get all the categories for this topic collection
                var categories = topics.Select(x => x.Category).Distinct();

                // create permissions
                var permissions = new Dictionary<Category, PermissionSet>();

                // loop through the categories and get the permissions
                foreach (var category in categories)
                {
                    var permissionSet = RoleService.GetPermissions(category, UsersRole);
                    permissions.Add(category, permissionSet);
                }

                // Now loop through the topics and remove any that user does not have permission for
                foreach (var topic in topics)
                {
                    // Get the permissions for this topic via its parent category
                    var permission = permissions[topic.Category];

                    // Add only topics user has permission to
                    if (!permission[AppConstants.PermissionDenyAccess].IsTicked)
                    {
                        if (topic.Posts.Any())
                        {
                            var firstOrDefault = topic.Posts.FirstOrDefault(x => x.IsTopicStarter);
                            if (firstOrDefault != null)
                                rssTopics.Add(new RssItem { Description = firstOrDefault.PostContent, Link = topic.NiceUrl, Title = topic.Name, PublishedDate = topic.CreateDate });
                        }
                    }
                }

                return new RssResult(rssTopics, LocalizationService.GetResourceString("Rss.LatestActivity.Title"), LocalizationService.GetResourceString("Rss.LatestActivity.Description"));
            }
        }

        [OutputCache(Duration = AppConstants.DefaultCacheLengthInSeconds)]
        public ActionResult ActivityRss()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                // get an rss lit ready
                var rssActivities = new List<RssItem>();

                var activities = _activityService.GetAll(20).OrderByDescending(x => x.ActivityMapped.Timestamp);

                var activityLink = Url.Action("Activity");

                // Now loop through the topics and remove any that user does not have permission for
                foreach (var activity in activities)
                {
                    if (activity is BadgeActivity)
                    {
                        var badgeActivity = activity as BadgeActivity;
                        rssActivities.Add(new RssItem
                            {
                                Description = badgeActivity.Badge.Description,
                                Title = string.Concat(badgeActivity.User.UserName, " ", LocalizationService.GetResourceString("Activity.UserAwardedBadge"), " ", badgeActivity.Badge.DisplayName, " ", LocalizationService.GetResourceString("Activity.Badge")),
                                PublishedDate = badgeActivity.ActivityMapped.Timestamp,
                                RssImage = AppHelpers.ReturnBadgeUrl(badgeActivity.Badge.Image),
                                Link = activityLink
                            });
                    }
                    else if (activity is MemberJoinedActivity)
                    {
                        var memberJoinedActivity = activity as MemberJoinedActivity;
                        rssActivities.Add(new RssItem
                        {
                            Description = string.Empty,
                            Title = LocalizationService.GetResourceString("Activity.UserJoined"),
                            PublishedDate = memberJoinedActivity.ActivityMapped.Timestamp,
                            RssImage = StringUtils.GetGravatarImage(memberJoinedActivity.User.Email, AppConstants.GravatarPostSize),
                            Link = activityLink
                        });
                    }
                    else if (activity is ProfileUpdatedActivity)
                    {
                        var profileUpdatedActivity = activity as ProfileUpdatedActivity;
                        rssActivities.Add(new RssItem
                        {
                            Description = string.Empty,
                            Title = LocalizationService.GetResourceString("Activity.ProfileUpdated"),
                            PublishedDate = profileUpdatedActivity.ActivityMapped.Timestamp,
                            RssImage = StringUtils.GetGravatarImage(profileUpdatedActivity.User.Email, AppConstants.GravatarPostSize),
                            Link = activityLink
                        });
                    }

                }

                return new RssResult(rssActivities, LocalizationService.GetResourceString("Rss.LatestActivity.Title"), LocalizationService.GetResourceString("Rss.LatestActivity.Description"));
            }
        }
    }
}
