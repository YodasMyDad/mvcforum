namespace MvcForum.Web.Areas.Admin.Controllers
{
    using System;
    using System.Web.Mvc;
    using Core.Constants;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using ViewModels;

    [Authorize(Roles = AppConstants.AdminRoleName)]
    public class AdminSocialController : BaseAdminController
    {
        private readonly ICacheService _cacheService;

        public AdminSocialController(ILoggingService loggingService, IMembershipService membershipService,
            ILocalizationService localizationService, ISettingsService settingsService, ICacheService cacheService,
            IMvcForumContext context)
            : base(loggingService, membershipService, localizationService, settingsService, context)
        {
            _cacheService = cacheService;
        }

        public ActionResult Index()
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

        [HttpPost]
        public ActionResult Index(SocialSettingsViewModel viewModel)
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
                Context.SaveChanges();
                _cacheService.ClearStartsWith(CacheKeys.Settings.Main);
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
                Context.RollBack();

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