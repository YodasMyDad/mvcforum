using System;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.Areas.Admin.ViewModels;

namespace MVCForum.Website.Areas.Admin.Controllers
{
    [Authorize(Roles = AppConstants.AdminRoleName)]
    public class AdminSocialController : BaseAdminController
    {
        private readonly ICacheService _cacheService;

        public AdminSocialController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, ILocalizationService localizationService, ISettingsService settingsService, ICacheService cacheService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, settingsService)
        {
            _cacheService = cacheService;
        }

        public ActionResult Index()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var settings = SettingsService.GetSettings();
                var viewModel = new SocialSettingsViewModel
                {
                    EnableSocialLogins = settings.EnableSocialLogins == true,
                    FacebookAppId = SiteConstants.Instance.FacebookAppId,
                    FacebookAppSecret = SiteConstants.Instance.FacebookAppSecret,
                    GooglePlusAppId = SiteConstants.Instance.GooglePlusAppId,
                    GooglePlusAppSecret = SiteConstants.Instance.GooglePlusAppSecret,
                    MicrosoftAppId = SiteConstants.Instance.MicrosoftAppId,
                    MicrosoftAppSecret = SiteConstants.Instance.MicrosoftAppSecret
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
                viewModel.FacebookAppId = SiteConstants.Instance.FacebookAppId;
                viewModel.FacebookAppSecret = SiteConstants.Instance.FacebookAppSecret;
                viewModel.GooglePlusAppId = SiteConstants.Instance.GooglePlusAppId;
                viewModel.GooglePlusAppSecret = SiteConstants.Instance.GooglePlusAppSecret;
                viewModel.MicrosoftAppId = SiteConstants.Instance.MicrosoftAppId;
                viewModel.MicrosoftAppSecret = SiteConstants.Instance.MicrosoftAppSecret;

                try
                {
                    unitOfWork.Commit();
                    _cacheService.ClearStartsWith(AppConstants.SettingsCacheName);
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