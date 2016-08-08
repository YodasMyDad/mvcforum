namespace MVCForum.Website.Controllers
{
    using System.Web.Mvc;
    using Domain.Interfaces.Services;
    using Domain.Interfaces.UnitOfWork;
    using ViewModels;

    public partial class SnippetsController : BaseController
    {
        private readonly IMembershipUserPointsService _membershipUserPointsService;

        public SnippetsController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, 
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService, 
            IMembershipUserPointsService membershipUserPointsService, ICacheService cacheService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService, cacheService)
        {
            _membershipUserPointsService = membershipUserPointsService;
        }

        public PartialViewResult GetThisWeeksTopEarners()
        {
            if(Request.IsAjaxRequest())
            {
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    var highEarners = _membershipUserPointsService.GetCurrentWeeksPoints(20);
                    var viewModel = new HighEarnersPointViewModel { HighEarners = highEarners };
                    return PartialView(viewModel); 
                }
            }
            return null;  
        }

        public PartialViewResult GetThisYearsTopEarners()
        {
            if (Request.IsAjaxRequest())
            {
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    var highEarners = _membershipUserPointsService.GetThisYearsPoints(20);
                    var viewModel = new HighEarnersPointViewModel { HighEarners = highEarners };
                    return PartialView(viewModel);
                }
            }
            return null;
        }
    }
}
