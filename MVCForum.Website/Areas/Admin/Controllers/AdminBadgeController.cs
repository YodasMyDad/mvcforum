namespace MvcForum.Web.Areas.Admin.Controllers
{
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Core;
    using Core.Constants;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Web.ViewModels.Admin;

    [Authorize(Roles = Constants.AdminRoleName)]
    public class AdminBadgeController : BaseAdminController
    {
        private readonly IBadgeService _badgeService;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="context"></param>
        /// <param name="membershipService"> </param>
        /// <param name="localizationService"></param>
        /// <param name="settingsService"> </param>
        /// <param name="badgeService"> </param>
        /// <param name="loggingService"> </param>
        public AdminBadgeController(IBadgeService badgeService, ILoggingService loggingService,
            IMvcForumContext context,
            IMembershipService membershipService, ILocalizationService localizationService,
            ISettingsService settingsService)
            : base(loggingService, membershipService, localizationService, settingsService, context)
        {
            _badgeService = badgeService;
        }

        /// <summary>
        ///     We get here via the admin default layout (_AdminLayout). The returned view is displayed by
        ///     the @RenderBody in that layout
        /// </summary>
        /// <returns></returns>
        public async Task<ActionResult> Index(int? p, string search)
        {
            var pageIndex = p ?? 1;


            var allBadges = string.IsNullOrWhiteSpace(search)
                ? await _badgeService.GetPagedGroupedBadges(pageIndex, ForumConfiguration.Instance.AdminListPageSize)
                : await _badgeService.SearchPagedGroupedTags(search, pageIndex,
                    ForumConfiguration.Instance.AdminListPageSize);

            var badgesListModel = new ListBadgesViewModel
            {
                Badges = allBadges,
                PageIndex = pageIndex,
                TotalCount = allBadges.TotalCount,
                Search = search
            };

            return View(badgesListModel);
        }

        public ActionResult Manage(int? p, string search)
        {
            return RedirectToAction("Index", new {p, search});
        }
    }
}