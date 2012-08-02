using System.Web.Mvc;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;

namespace MVCForum.Website.Controllers
{
    public class ClosedController : BaseController
    {
        public ClosedController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, 
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService) : 
            base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
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
