namespace MvcForum.Web.Controllers
{
    using System.Web.Mvc;
    using Core.DomainModel.Enums;
    using Core.Interfaces.Services;
    using Core.Interfaces.UnitOfWork;
    using ViewModels;

    public partial class PointController : BaseController
    {
        private readonly IMembershipUserPointsService _membershipUserPointsService;

        public PointController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager,
            IMembershipService membershipService,
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService,
            IMembershipUserPointsService membershipUserPointsService, ICacheService cacheService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService,
                settingsService, cacheService)
        {
            _membershipUserPointsService = membershipUserPointsService;
        }

        [ChildActionOnly]
        [OutputCache(Duration = (int) CacheTimes.TwoHours)]
        public PartialViewResult CurrentWeekHighPointUsers()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var highEarners = _membershipUserPointsService.GetCurrentWeeksPoints(10);
                var viewModel = new HighEarnersPointViewModel {HighEarners = highEarners};
                return PartialView(viewModel);
            }
        }
    }
}