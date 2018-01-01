namespace MvcForum.Web.Areas.Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using Core.Constants;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Core.Models.General;
    using ViewModels;

    [Authorize(Roles = AppConstants.AdminRoleName)]
    public class LogController : BaseAdminController
    {
        public LogController(ILoggingService loggingService, IMembershipService membershipService,
            ILocalizationService localizationService, ISettingsService settingsService, IMvcForumContext context) :
            base(loggingService, membershipService, localizationService, settingsService, context)
        {
        }

        public ActionResult Index()
        {
            IList<LogEntry> logs = new List<LogEntry>();

            try
            {
                logs = LoggingService.ListLogFile();
            }
            catch (Exception ex)
            {
                var err = $"Unable to access logs: {ex.Message}";
                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = err,
                    MessageType = GenericMessages.danger
                };

                LoggingService.Error(err);
            }

            return View(new ListLogViewModel {LogFiles = logs});
        }

        public ActionResult ClearLog()
        {
            LoggingService.ClearLogFiles();

            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = "Log File Cleared",
                MessageType = GenericMessages.success
            };
            return RedirectToAction("Index");
        }
    }
}