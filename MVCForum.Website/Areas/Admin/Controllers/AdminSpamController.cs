using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;

namespace MVCForum.Website.Areas.Admin.Controllers
{
    public class AdminSpamController : BaseAdminController
    {
        // GET: Admin/AdminSpam
        public AdminSpamController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, ILocalizationService localizationService, ISettingsService settingsService) : base(loggingService, unitOfWorkManager, membershipService, localizationService, settingsService)
        {
        }

        public ActionResult Akismet()
        {
            return View();
        }

        public ActionResult RegistrationQuestion()
        {
            return View();
        }

        public ActionResult SpamReporting()
        {
            return View();
        }
    }
}