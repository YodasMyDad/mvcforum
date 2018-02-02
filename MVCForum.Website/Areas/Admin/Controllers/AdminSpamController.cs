namespace MvcForum.Web.Areas.Admin.Controllers
{
    using System;
    using System.Web.Mvc;
    using Core.Constants;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Core.Models.Entities;
    using Web.ViewModels;
    using Web.ViewModels.Admin;

    [Authorize(Roles = Constants.AdminRoleName)]
    public class AdminSpamController : BaseAdminController
    {
        private readonly ICacheService _cacheService;
        private readonly Settings _settings;

        public AdminSpamController(ILoggingService loggingService, IMembershipService membershipService,
            ILocalizationService localizationService, ISettingsService settingsService, ICacheService cacheService,
            IMvcForumContext context)
            : base(loggingService, membershipService, localizationService, settingsService, context)
        {
            _cacheService = cacheService;
            _settings = SettingsService.GetSettings();
        }

        public ActionResult Akismet()
        {
            var viewModel = new AkismetViewModel
            {
                AkismentKey = _settings.AkismentKey,
                EnableAkisment = _settings.EnableAkisment == true
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult Akismet(AkismetViewModel viewModel)
        {
            var settings = SettingsService.GetSettings(false);
            settings.AkismentKey = viewModel.AkismentKey;
            settings.EnableAkisment = viewModel.EnableAkisment;

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
                    Message = "Error, Please check log",
                    MessageType = GenericMessages.danger
                });
            }

            return View(viewModel);
        }

        public ActionResult RegistrationQuestion()
        {
            var viewModel = new RegistrationQuestionViewModel
            {
                SpamAnswer = _settings.SpamAnswer,
                SpamQuestion = _settings.SpamQuestion
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult RegistrationQuestion(RegistrationQuestionViewModel viewModel)
        {
            var settings = SettingsService.GetSettings(false);
            settings.SpamAnswer = viewModel.SpamAnswer;
            settings.SpamQuestion = viewModel.SpamQuestion;

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
                    Message = "Error, Please check log",
                    MessageType = GenericMessages.danger
                });
            }

            return View(viewModel);
        }

        public ActionResult SpamReporting()
        {
            var viewModel = new SpamReportingViewModel
            {
                EnableMemberReporting = _settings.EnableMemberReporting,
                EnableSpamReporting = _settings.EnableSpamReporting
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult SpamReporting(SpamReportingViewModel viewModel)
        {
            var settings = SettingsService.GetSettings(false);
            settings.EnableMemberReporting = viewModel.EnableMemberReporting;
            settings.EnableSpamReporting = viewModel.EnableSpamReporting;

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
                    Message = "Error, Please check log",
                    MessageType = GenericMessages.danger
                });
            }

            return View(viewModel);
        }
    }
}