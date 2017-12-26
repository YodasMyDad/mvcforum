namespace MvcForum.Web.Areas.Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using Core.Constants;
    using Core.DomainModel.General;
    using Core.Interfaces.Services;
    using Core.Interfaces.UnitOfWork;
    using ViewModels;

    [Authorize(Roles = AppConstants.AdminRoleName)]
    public class LogController : BaseAdminController
    {
        public LogController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager,
            IMembershipService membershipService, ILocalizationService localizationService,
            ISettingsService settingsService) :
            base(loggingService, unitOfWorkManager, membershipService, localizationService, settingsService)
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