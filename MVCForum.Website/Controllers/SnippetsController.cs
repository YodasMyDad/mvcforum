namespace MvcForum.Web.Controllers
{
    using System.Web.Mvc;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using ViewModels;

    public partial class SnippetsController : BaseController
    {
        private readonly IMembershipUserPointsService _membershipUserPointsService;

        public SnippetsController(ILoggingService loggingService, IMembershipService membershipService,
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService,
            IMembershipUserPointsService membershipUserPointsService, ICacheService cacheService,
            IMvcForumContext context)
            : base(loggingService, membershipService, localizationService, roleService,
                settingsService, cacheService, context)
        {
            _membershipUserPointsService = membershipUserPointsService;
        }

        public virtual PartialViewResult GetThisWeeksTopEarners()
        {
            if (Request.IsAjaxRequest())
            {
                var highEarners = _membershipUserPointsService.GetCurrentWeeksPoints(20);
                var viewModel = new HighEarnersPointViewModel {HighEarners = highEarners};
                return PartialView(viewModel);
            }
            return null;
        }

        public virtual PartialViewResult GetThisYearsTopEarners()
        {
            if (Request.IsAjaxRequest())
            {
                var highEarners = _membershipUserPointsService.GetThisYearsPoints(20);
                var viewModel = new HighEarnersPointViewModel {HighEarners = highEarners};
                return PartialView(viewModel);
            }
            return null;
        }
    }
}