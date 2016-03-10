using System;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.Areas.Admin.ViewModels;

namespace MVCForum.Website.Areas.Admin.Controllers
{
    [Authorize(Roles = AppConstants.AdminRoleName)]
    public class AdminSpamController : BaseAdminController
    {
        private readonly Settings _settings;
        private readonly ICacheService _cacheService;

        public AdminSpamController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, ILocalizationService localizationService, ISettingsService settingsService, ICacheService cacheService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, settingsService)
        {
            _cacheService = cacheService;
            _settings = SettingsService.GetSettings();
        }

        public ActionResult Akismet()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var viewModel = new AkismetViewModel
                {
                    AkismentKey = _settings.AkismentKey,
                    EnableAkisment = _settings.EnableAkisment == true
                };
                return View(viewModel);
            }
        }

        [HttpPost]
        public ActionResult Akismet(AkismetViewModel viewModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var settings = SettingsService.GetSettings(false);
                settings.AkismentKey = viewModel.AkismentKey;
                settings.EnableAkisment = viewModel.EnableAkisment;

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
                        Message = "Error, Please check log",
                        MessageType = GenericMessages.danger
                    });
                }

                return View(viewModel);
            }
        }

        public ActionResult RegistrationQuestion()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var viewModel = new RegistrationQuestionViewModel
                {
                    SpamAnswer = _settings.SpamAnswer, SpamQuestion = _settings.SpamQuestion
                };
                return View(viewModel);
            }
        }

        [HttpPost]
        public ActionResult RegistrationQuestion(RegistrationQuestionViewModel viewModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var settings = SettingsService.GetSettings(false);
                settings.SpamAnswer = viewModel.SpamAnswer;
                settings.SpamQuestion = viewModel.SpamQuestion;

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
                        Message = "Error, Please check log",
                        MessageType = GenericMessages.danger
                    });
                }

                return View(viewModel);
            }
        }

        public ActionResult SpamReporting()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var viewModel = new SpamReportingViewModel
                {
                    EnableMemberReporting = _settings.EnableMemberReporting, EnableSpamReporting = _settings.EnableSpamReporting
                };
                return View(viewModel);
            }
        }

        [HttpPost]
        public ActionResult SpamReporting(SpamReportingViewModel viewModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var settings = SettingsService.GetSettings(false);
                settings.EnableMemberReporting = viewModel.EnableMemberReporting;
                settings.EnableSpamReporting = viewModel.EnableSpamReporting;

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
                        Message = "Error, Please check log",
                        MessageType = GenericMessages.danger
                    });
                }

                return View(viewModel);
            }
        }
    }
}