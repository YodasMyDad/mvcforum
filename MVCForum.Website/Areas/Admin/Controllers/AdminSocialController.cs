using System;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.Application;
using MVCForum.Website.Areas.Admin.ViewModels;

namespace MVCForum.Website.Areas.Admin.Controllers
{
    [Authorize(Roles = AppConstants.AdminRoleName)]
    public class AdminSocialController : BaseAdminController
    {
        public AdminSocialController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, ILocalizationService localizationService, ISettingsService settingsService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, settingsService)
        {
        }

        public ActionResult Index()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var settings = SettingsService.GetSettings();
                var viewModel = new SocialSettingsViewModel
                {
                    EnableSocialLogins = settings.EnableSocialLogins == true,
                    FacebookAppId = SiteConstants.FacebookAppId,
                    FacebookAppSecret = SiteConstants.FacebookAppSecret,
                    GooglePlusAppId = SiteConstants.GooglePlusAppId,
                    GooglePlusAppSecret = SiteConstants.GooglePlusAppSecret,
                    MicrosoftAppId = SiteConstants.MicrosoftAppId,
                    MicrosoftAppSecret = SiteConstants.MicrosoftAppSecret
                };
                return View(viewModel);
            }
        }

        [HttpPost]
        public ActionResult Index(SocialSettingsViewModel viewModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
 
                var settings = SettingsService.GetSettings(false);

                settings.EnableSocialLogins = viewModel.EnableSocialLogins;

                // Repopulate the view model
                viewModel.FacebookAppId = SiteConstants.FacebookAppId;
                viewModel.FacebookAppSecret = SiteConstants.FacebookAppSecret;
                viewModel.GooglePlusAppId = SiteConstants.GooglePlusAppId;
                viewModel.GooglePlusAppSecret = SiteConstants.GooglePlusAppSecret;
                viewModel.MicrosoftAppId = SiteConstants.MicrosoftAppId;
                viewModel.MicrosoftAppSecret = SiteConstants.MicrosoftAppSecret;

                try
                {
                    unitOfWork.Commit();

                    // Show a message
                    ShowMessage(new GenericMessageViewModel
                    {
                        Message = "Updated",
                        MessageType = GenericMessages.success
                    });

                }
                catch (Exception ex)
                {
                    LoggingService.Error(ex);
                    unitOfWork.Rollback();

                    // Show a message
                    ShowMessage(new GenericMessageViewModel
                    {
                        Message = "Error, please check log",
                        MessageType = GenericMessages.danger
                    });
                }

                return View(viewModel); 
            }
        }
    }
}