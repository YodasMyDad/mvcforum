namespace MvcForum.Web.Areas.Admin.Controllers
{
    using System;
    using System.Web.Mvc;
    using Core;
    using Core.Constants;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Web.ViewModels;
    using Web.ViewModels.Admin;

    [Authorize(Roles = Constants.AdminRoleName)]
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
                FacebookAppId = ForumConfiguration.Instance.FacebookAppId,
                FacebookAppSecret = ForumConfiguration.Instance.FacebookAppSecret,
                GooglePlusAppId = ForumConfiguration.Instance.GooglePlusAppId,
                GooglePlusAppSecret = ForumConfiguration.Instance.GooglePlusAppSecret,
                MicrosoftAppId = ForumConfiguration.Instance.MicrosoftAppId,
                MicrosoftAppSecret = ForumConfiguration.Instance.MicrosoftAppSecret
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Index(SocialSettingsViewModel viewModel)
        {
            var settings = SettingsService.GetSettings(false);

            settings.EnableSocialLogins = viewModel.EnableSocialLogins;

            // Repopulate the view model
            viewModel.FacebookAppId = ForumConfiguration.Instance.FacebookAppId;
            viewModel.FacebookAppSecret = ForumConfiguration.Instance.FacebookAppSecret;
            viewModel.GooglePlusAppId = ForumConfiguration.Instance.GooglePlusAppId;
            viewModel.GooglePlusAppSecret = ForumConfiguration.Instance.GooglePlusAppSecret;
            viewModel.MicrosoftAppId = ForumConfiguration.Instance.MicrosoftAppId;
            viewModel.MicrosoftAppSecret = ForumConfiguration.Instance.MicrosoftAppSecret;

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