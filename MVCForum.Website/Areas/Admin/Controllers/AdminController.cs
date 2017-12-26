namespace MvcForum.Web.Areas.Admin.Controllers
{
    using System.Web.Mvc;
    using Core.Constants;
    using Core.Interfaces.Services;
    using Core.Interfaces.UnitOfWork;
    using Core.Utilities;

    [Authorize(Roles = AppConstants.AdminRoleName)]
    public class AdminController : BaseAdminController
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
        ///     Default page for the admin area
        /// </summary>
        /// <returns></returns>
        public ActionResult Index()
        {
            return View();
        }

        #region Utils

        [HttpPost]
        public void Aptitude()
        {
            try
            {
                var url = StringUtils.ReturnCurrentDomain();
                var postString = string.Concat("url=", url);
                var response = StringUtils.PostForm("http://www.mvcforum.com/base/MVCBase/DomainCheck", postString);
            }
            catch
            {
                // No need to do anything
            }
        }

        #endregion
    }
}