namespace MvcForum.Web.Controllers
{
    using System;
    using System.Web.Mvc;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Core.Models.Enums;
    using ViewModels;
    using ViewModels.Badge;

    public partial class BadgeController : BaseController
    {
        private readonly IBadgeService _badgeService;
        private readonly IFavouriteService _favouriteService;
        private readonly IPostService _postService;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="loggingService"> </param>
        /// <param name="badgeService"> </param>
        /// <param name="postService"> </param>
        /// <param name="membershipService"> </param>
        /// <param name="localizationService"></param>
        /// <param name="roleService"> </param>
        /// <param name="settingsService"> </param>
        /// <param name="favouriteService"></param>
        /// <param name="cacheService"></param>
        /// <param name="context"></param>
        public BadgeController(ILoggingService loggingService, IBadgeService badgeService, IPostService postService,
            IMembershipService membershipService, ILocalizationService localizationService, IRoleService roleService,
            ISettingsService settingsService, IFavouriteService favouriteService, ICacheService cacheService,
            IMvcForumContext context)
            : base(loggingService, membershipService, localizationService, roleService,
                settingsService, cacheService, context)
        {
            _badgeService = badgeService;
            _postService = postService;
            _favouriteService = favouriteService;
        }


        [HttpPost]
        [Authorize]
        public void VoteUpPost(EntityIdViewModel voteUpBadgeViewModel)
        {
            try
            {
                var loggedOnUser = User.GetMembershipUser(MembershipService, false);
                var databaseUpdateNeededOne = _badgeService.ProcessBadge(BadgeType.VoteUp, loggedOnUser);
                if (databaseUpdateNeededOne)
                {
                    Context.SaveChanges();
                }

                var post = _postService.Get(voteUpBadgeViewModel.Id);
                var databaseUpdateNeededTwo = _badgeService.ProcessBadge(BadgeType.VoteUp, post.User);
                if (databaseUpdateNeededTwo)
                {
                    Context.SaveChanges();
                }

                if (databaseUpdateNeededOne || databaseUpdateNeededTwo)
                {
                    Context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
            }
        }

        [HttpPost]
        [Authorize]
        public void VoteDownPost(EntityIdViewModel voteUpBadgeViewModel)
        {
            try
            {
                var loggedOnUser = User.GetMembershipUser(MembershipService, false);
                var databaseUpdateNeededOne = _badgeService.ProcessBadge(BadgeType.VoteDown, loggedOnUser);
                if (databaseUpdateNeededOne)
                {
                    Context.SaveChanges();
                }

                var post = _postService.Get(voteUpBadgeViewModel.Id);
                var databaseUpdateNeededTwo = _badgeService.ProcessBadge(BadgeType.VoteDown, post.User);

                if (databaseUpdateNeededTwo)
                {
                    Context.SaveChanges();
                }

                if (databaseUpdateNeededOne || databaseUpdateNeededTwo)
                {
                    Context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
            }
        }

        [HttpPost]
        [Authorize]
        public void Post()
        {
            if (Request.IsAjaxRequest())
            {
                try
                {
                    var loggedOnUser = User.GetMembershipUser(MembershipService, false);
                    var databaseUpdateNeeded = _badgeService.ProcessBadge(BadgeType.Post, loggedOnUser);

                    if (databaseUpdateNeeded)
                    {
                        Context.SaveChanges();
                    }
                }
                catch (Exception ex)
                {
                    Context.RollBack();
                    LoggingService.Error(ex);
                }
            }
        }

        [HttpPost]
        [Authorize]
        public void MarkAsSolution(EntityIdViewModel markAsSolutionBadgeViewModel)
        {
            try
            {
                var post = _postService.Get(markAsSolutionBadgeViewModel.Id);
                var databaseUpdateNeeded = _badgeService.ProcessBadge(BadgeType.MarkAsSolution, post.User) |
                                           _badgeService.ProcessBadge(BadgeType.MarkAsSolution, post.Topic.User);

                if (databaseUpdateNeeded)
                {
                    Context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
            }
        }

        [HttpPost]
        [Authorize]
        public void Favourite(EntityIdViewModel favouriteViewModel)
        {
            try
            {
                var favourite = _favouriteService.Get(favouriteViewModel.Id);
                var databaseUpdateNeeded = _badgeService.ProcessBadge(BadgeType.Favourite, favourite.Member) |
                                           _badgeService.ProcessBadge(BadgeType.Favourite, favourite.Post.User);

                if (databaseUpdateNeeded)
                {
                    Context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
            }
        }

        [HttpPost]
        [Authorize]
        public void ProfileBadgeCheck()
        {
            try
            {
                var loggedOnUser = User.GetMembershipUser(MembershipService, false);
                if (loggedOnUser != null)
                {
                    var databaseUpdateNeeded = _badgeService.ProcessBadge(BadgeType.Profile, loggedOnUser);

                    if (databaseUpdateNeeded)
                    {
                        Context.SaveChanges();
                    }
                }
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
            }
        }

        [HttpPost]
        public void Time(EntityIdViewModel timeBadgeViewModel)
        {
            try
            {
                var user = MembershipService.GetUser(timeBadgeViewModel.Id);
                var databaseUpdateNeeded = _badgeService.ProcessBadge(BadgeType.Time, user);

                if (databaseUpdateNeeded)
                {
                    Context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
            }
        }

        public ActionResult AllBadges()
        {
            var allBadges = _badgeService.GetallBadges();

            // Localise the badge names
            foreach (var item in allBadges)
            {
                var partialKey = string.Concat("Badge.", item.Name);
                item.DisplayName = LocalizationService.GetResourceString(string.Concat(partialKey, ".Name"));
                item.Description = LocalizationService.GetResourceString(string.Concat(partialKey, ".Desc"));
            }

            var badgesListModel = new AllBadgesViewModel
            {
                AllBadges = allBadges
            };

            return View(badgesListModel);
        }
    }
}