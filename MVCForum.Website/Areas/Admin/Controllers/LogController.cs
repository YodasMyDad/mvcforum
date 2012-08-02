using System;
using System.Collections.Generic;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.Areas.Admin.ViewModels;

namespace MVCForum.Website.Areas.Admin.Controllers
{
    [Authorize(Roles = AppConstants.AdminRoleName)]
    public class LogController : BaseAdminController
    {
        public LogController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, ILocalizationService localizationService, ISettingsService settingsService) :
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
                var err = string.Format("Unable to access logs: {0}", ex.Message);
                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = err,
                    MessageType = GenericMessages.error
                };

                LoggingService.Error(err);
            }
                       
            return View(new ListLogViewModel{LogFiles = logs});
        }

    }
}
