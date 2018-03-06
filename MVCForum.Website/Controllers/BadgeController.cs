namespace MvcForum.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
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
        public virtual async Task<ActionResult> VoteUpPost(EntityIdViewModel voteUpBadgeViewModel)
        {
            try
            {
                var loggedOnUser = User.GetMembershipUser(MembershipService, false);
                var databaseUpdateNeededOne = await _badgeService.ProcessBadge(BadgeType.VoteUp, loggedOnUser);
                if (databaseUpdateNeededOne)
                {
                    await Context.SaveChangesAsync();
                }

                var post = _postService.Get(voteUpBadgeViewModel.Id);
                var databaseUpdateNeededTwo = await _badgeService.ProcessBadge(BadgeType.VoteUp, post.User);
                if (databaseUpdateNeededTwo)
                {
                    await Context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
            }

            // TODO - Should be returning something meaningful!
            return Content(string.Empty);
        }

        [HttpPost]
        [Authorize]
        public virtual async Task<ActionResult> VoteDownPost(EntityIdViewModel voteUpBadgeViewModel)
        {
            try
            {
                var loggedOnUser = User.GetMembershipUser(MembershipService, false);
                var databaseUpdateNeededOne = await _badgeService.ProcessBadge(BadgeType.VoteDown, loggedOnUser);
                if (databaseUpdateNeededOne)
                {
                    await Context.SaveChangesAsync();
                }

                var post = _postService.Get(voteUpBadgeViewModel.Id);
                var databaseUpdateNeededTwo = await _badgeService.ProcessBadge(BadgeType.VoteDown, post.User);
                if (databaseUpdateNeededTwo)
                {
                    await Context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
            }

            // TODO - Should be returning something meaningful!
            return Content(string.Empty);
        }

        [HttpPost]
        [Authorize]
        public virtual async Task<ActionResult> Post()
        {
            if (Request.IsAjaxRequest())
            {
                try
                {
                    var loggedOnUser = User.GetMembershipUser(MembershipService, false);
                    var databaseUpdateNeeded = await _badgeService.ProcessBadge(BadgeType.Post, loggedOnUser);

                    if (databaseUpdateNeeded)
                    {
                        await Context.SaveChangesAsync();
                    }
                }
                catch (Exception ex)
                {
                    Context.RollBack();
                    LoggingService.Error(ex);
                }
            }


            // TODO - Should be returning something meaningful!
            return Content(string.Empty);
        }

        [HttpPost]
        [Authorize]
        public virtual async Task<ActionResult> MarkAsSolution(EntityIdViewModel markAsSolutionBadgeViewModel)
        {
            try
            {
                var post = _postService.Get(markAsSolutionBadgeViewModel.Id);

                bool databaseUpdateNeeded;

                if (post.User != post.Topic.User)
                {
                    databaseUpdateNeeded = await _badgeService.ProcessBadge(BadgeType.MarkAsSolution, post.User) | await _badgeService.ProcessBadge(BadgeType.MarkAsSolution, post.Topic.User);
                }
                else
                {
                    databaseUpdateNeeded = await _badgeService.ProcessBadge(BadgeType.MarkAsSolution, post.User);
                }

                if (databaseUpdateNeeded)
                {
                    await Context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
            }

            // TODO - Should be returning something meaningful!
            return Content(string.Empty);
        }

        [HttpPost]
        [Authorize]
        public virtual async Task<ActionResult> Favourite(EntityIdViewModel favouriteViewModel)
        {
            try
            {
                var favourite = _favouriteService.Get(favouriteViewModel.Id);
                var databaseUpdateNeeded = await _badgeService.ProcessBadge(BadgeType.Favourite, favourite.Member) |
                                           await _badgeService.ProcessBadge(BadgeType.Favourite, favourite.Post.User);

                if (databaseUpdateNeeded)
                {
                    await Context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
            }

            // TODO - Should be returning something meaningful!
            return Content(string.Empty);
        }

        [HttpPost]
        [Authorize]
        public virtual async Task<ActionResult> ProfileBadgeCheck()
        {
            try
            {
                var loggedOnUser = User.GetMembershipUser(MembershipService, false);
                if (loggedOnUser != null)
                {
                    var databaseUpdateNeeded = await _badgeService.ProcessBadge(BadgeType.Profile, loggedOnUser);

                    if (databaseUpdateNeeded)
                    {
                        await Context.SaveChangesAsync();
                    }
                }
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
            }

            // TODO - Should be returning something meaningful!
            return Content(string.Empty);
        }

        [HttpPost]
        public virtual async Task<ActionResult> Time(EntityIdViewModel timeBadgeViewModel)
        {
            try
            {
                var user = MembershipService.GetUser(timeBadgeViewModel.Id);
                var databaseUpdateNeeded = await _badgeService.ProcessBadge(BadgeType.Time, user);

                if (databaseUpdateNeeded)
                {
                    await Context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
            }

            // TODO - Should be returning something meaningful!
            return Content(string.Empty);
        }

        public ActionResult AllBadges()
        {
            var allBadges = _badgeService.GetAll().ToList();

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