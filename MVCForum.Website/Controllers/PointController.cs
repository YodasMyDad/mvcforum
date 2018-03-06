namespace MvcForum.Web.Controllers
{
    using System.Web.Mvc;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Core.Models.Enums;
    using ViewModels;

    public partial class PointController : BaseController
    {
        private readonly IMembershipUserPointsService _membershipUserPointsService;

        public PointController(ILoggingService loggingService, IMembershipService membershipService,
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService,
            IMembershipUserPointsService membershipUserPointsService, ICacheService cacheService,
            IMvcForumContext context)
            : base(loggingService, membershipService, localizationService, roleService,
                settingsService, cacheService, context)
        {
            _membershipUserPointsService = membershipUserPointsService;
        }

        [ChildActionOnly]
        [OutputCache(Duration = (int) CacheTimes.TwoHours)]
        public virtual PartialViewResult CurrentWeekHighPointUsers()
        {
            var highEarners = _membershipUserPointsService.GetCurrentWeeksPoints(10);
            var viewModel = new HighEarnersPointViewModel {HighEarners = highEarners};
            return PartialView(viewModel);
        }
    }
}