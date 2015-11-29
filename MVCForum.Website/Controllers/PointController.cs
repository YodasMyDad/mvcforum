using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel.Enums;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.ViewModels;

namespace MVCForum.Website.Controllers
{
    public partial class PointController : BaseController
    {
        private readonly IMembershipUserPointsService _membershipUserPointsService;

        public PointController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, 
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService,
            IMembershipUserPointsService membershipUserPointsService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _membershipUserPointsService = membershipUserPointsService;
        }

        [ChildActionOnly]
        [OutputCache(Duration = (int)CacheTimes.TwoHours)]
        public PartialViewResult CurrentWeekHighPointUsers()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var highEarners = _membershipUserPointsService.GetCurrentWeeksPoints(10);
                var viewModel = new HighEarnersPointViewModel { HighEarners = highEarners };
                return PartialView(viewModel);
            }
        }
    }
}
