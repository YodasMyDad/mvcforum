namespace MVCForum.Website.Controllers
{
    using System.Web.Mvc;
    using Domain.Interfaces.Services;
    using Domain.Interfaces.UnitOfWork;

    public partial class ClosedController : BaseController
    {
        public ClosedController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, 
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService, ICacheService cacheService) : 
            base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService, cacheService)
        {
        }

        /// <summary>
        /// This is called when the forum is closed
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

    }
}
