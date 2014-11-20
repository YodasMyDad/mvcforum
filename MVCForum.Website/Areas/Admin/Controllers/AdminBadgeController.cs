using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.Areas.Admin.ViewModels;

namespace MVCForum.Website.Areas.Admin.Controllers
{
    [Authorize(Roles = AppConstants.AdminRoleName)]
    public partial class AdminBadgeController : BaseAdminController
    {
        private readonly IBadgeService _badgeService;
        private readonly IPostService _postService;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="unitOfWorkManager"> </param>
        /// <param name="membershipService"> </param>
        /// <param name="localizationService"></param>
        /// <param name="settingsService"> </param>
        /// <param name="badgeService"> </param>
        /// <param name="loggingService"> </param>
        public AdminBadgeController(IBadgeService badgeService, IPostService postService, ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, 
            IMembershipService membershipService, ILocalizationService localizationService, ISettingsService settingsService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, settingsService)
        {
            _badgeService = badgeService;
            _postService = postService;
        }

        /// <summary>
        /// We get here via the admin default layout (_AdminLayout). The returned view is displayed by
        /// the @RenderBody in that layout
        /// </summary>
        /// <returns></returns>
        public ActionResult Index(int? p, string search)
        {
            var pageIndex = p ?? 1;

            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var allBadges = string.IsNullOrEmpty(search) ? _badgeService.GetPagedGroupedBadges(pageIndex, AppConstants.AdminListPageSize) :
                            _badgeService.SearchPagedGroupedTags(search, pageIndex, AppConstants.AdminListPageSize);

                var badgesListModel = new ListBadgesViewModel
                {
                    Badges = allBadges,
                    PageIndex = pageIndex,
                    TotalCount = allBadges.TotalCount,
                    Search = search
                };

                return View(badgesListModel);
            }

        }
        public ActionResult Rebuild()
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                bool upd = false;
                foreach (var post in _postService.GetAll())
                {
                    upd = upd || _badgeService.ProcessBadge(BadgeType.Post, post.User);

                    foreach (var vote in post.Votes)
                    {
                        upd = upd || _badgeService.ProcessBadge(BadgeType.VoteUp, vote.VotedByMembershipUser);
                        upd = upd || _badgeService.ProcessBadge(BadgeType.VoteUp, post.User);
                    }
                    if (post.IsSolution)
                    {
                        upd = upd || _badgeService.ProcessBadge(BadgeType.MarkAsSolution, post.User);
                        upd = upd || _badgeService.ProcessBadge(BadgeType.MarkAsSolution, post.Topic.User);
                    }
                }
                foreach(var user in MembershipService.GetAll())
                {
                    upd = upd || _badgeService.ProcessBadge(BadgeType.Time, user);
                }
                if (upd)
                {
                    unitOfWork.Commit();
                }
                
                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = string.Format("Votes recalculated!"),
                    MessageType = GenericMessages.success
                };
            }
            return RedirectToAction("Index");
        }

        public ActionResult Manage(int? p, string search)
        {
            return RedirectToAction("Index", new { p, search });
        }
    }
}
