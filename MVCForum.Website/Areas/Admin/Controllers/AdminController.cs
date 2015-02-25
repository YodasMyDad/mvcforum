using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Utilities;

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
        public ActionResult Index()
        {
            return View();
        }

        [HttpPost]
        public void Aptitude()
        {
            var url = StringUtils.ReturnCurrentDomain();
            var postString = string.Concat("url=", url);
            var response = StringUtils.PostForm("http://www.mvcforum.com/base/MVCBase/DomainCheck", postString);
        }

    }
}
