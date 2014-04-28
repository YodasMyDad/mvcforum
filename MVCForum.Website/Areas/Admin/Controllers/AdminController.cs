using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;

namespace MVCForum.Website.Areas.Admin.Controllers
{
    [Authorize(Roles = AppConstants.AdminRoleName)]
    public partial class AdminController : BaseAdminController
    {

        public AdminController(ILoggingService loggingService, 
            IUnitOfWorkManager unitOfWorkManager, 
            IMembershipService membershipService, 
            ILocalizationService localizationService, 
            ISettingsService settingsService) 
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, settingsService)
        {
        }

        /// <summary>
        /// Default page for the admin area
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

    }
}
