using System;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.Application;
using MVCForum.Website.Areas.Admin.ViewModels;
using MVCForum.Website.ViewModels.Mapping;

namespace MVCForum.Website.Areas.Admin.Controllers
{
    [Authorize(Roles = AppConstants.AdminRoleName)]
    public class SettingsController : BaseAdminController
    {
        private readonly IRoleService _roleService;
        private readonly IEmailService _emailService;
        private readonly ICacheService _cacheService;

        public SettingsController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager,
            ILocalizationService localizationService,
            IMembershipService membershipService,
            IRoleService roleService,
            ISettingsService settingsService, IEmailService emailService, ICacheService cacheService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, settingsService)
        {
            _roleService = roleService;
            _emailService = emailService;
            _cacheService = cacheService;
        }

        public ActionResult Index()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var currentSettings = SettingsService.GetSettings();
                var settingViewModel = ViewModelMapping.SettingsToSettingsViewModel(currentSettings);
                settingViewModel.NewMemberStartingRole = _roleService.GetRole(currentSettings.NewMemberStartingRole.Id).Id;
                settingViewModel.DefaultLanguage = LocalizationService.DefaultLanguage.Id;
                settingViewModel.Roles = _roleService.AllRoles().ToList();
                settingViewModel.Languages = LocalizationService.AllLanguages.ToList();
                return View(settingViewModel);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(EditSettingsViewModel settingsViewModel)
        {
            if (ModelState.IsValid)
            {
                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    try
                    {                        
                        var existingSettings = SettingsService.GetSettings(false);
                        var updatedSettings = ViewModelMapping.SettingsViewModelToSettings(settingsViewModel, existingSettings);

                        // Map over viewModel from 
                        if (settingsViewModel.NewMemberStartingRole != null)
                        {
                            updatedSettings.NewMemberStartingRole =
                                _roleService.GetRole(settingsViewModel.NewMemberStartingRole.Value);
                        }
                        
                        if (settingsViewModel.DefaultLanguage != null)
                        {
                            updatedSettings.DefaultLanguage =
                                LocalizationService.Get(settingsViewModel.DefaultLanguage.Value);
                        }

                        unitOfWork.Commit();
                        _cacheService.ClearStartsWith(CacheKeys.Settings.Main);
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                    }
                }

                // All good clear cache and get reliant lists
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = "Settings Updated",
                        MessageType = GenericMessages.success
                    };
                    settingsViewModel.Themes = AppHelpers.GetThemeFolders();
                    settingsViewModel.Roles = _roleService.AllRoles().ToList();
                    settingsViewModel.Languages = LocalizationService.AllLanguages.ToList();                   
                }
            }
            return View(settingsViewModel);
        }

        public ActionResult TestEmail()
        {
            return View();
        }

        [HttpPost]
        public ActionResult SendTestEmail()
        {
            using (var uow = UnitOfWorkManager.NewUnitOfWork())
            {
                var settings = SettingsService.GetSettings();
                var sb = new StringBuilder();
                sb.Append($"<p>{string.Concat("This is a test email from ", settings.ForumName)}</p>");
                var email = new Email
                {
                    EmailTo = settings.AdminEmailAddress,
                    NameTo = "Email Test Admin",
                    Subject = string.Concat("Email Test From ", settings.ForumName)
                };
                email.Body = _emailService.EmailTemplate(email.NameTo, sb.ToString());
                _emailService.SendMail(email);

                var message = new GenericMessageViewModel
                {
                    Message = "Test Email Sent",
                    MessageType = GenericMessages.success
                };

                try
                {
                    uow.Commit();
                }
                catch (Exception ex)
                {
                    uow.Rollback();
                    LoggingService.Error(ex);
                    message.Message = "Error sending email";
                    message.MessageType = GenericMessages.danger;
                }
                TempData[AppConstants.MessageViewBagName] = message;

                return RedirectToAction("Index");
            }
        }

        public ActionResult CustomCode()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var settings = SettingsService.GetSettings();
                var viewModel = new CustomCodeViewModels
                {
                    CustomFooterCode = settings.CustomFooterCode,
                    CustomHeaderCode = settings.CustomHeaderCode
                };
                return View(viewModel);
            }
        }

        [HttpPost]
        public ActionResult CustomCode(CustomCodeViewModels viewModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {

                var settings = SettingsService.GetSettings(false);

                settings.CustomFooterCode = viewModel.CustomFooterCode;
                settings.CustomHeaderCode = viewModel.CustomHeaderCode;

                try
                {
                    unitOfWork.Commit();

                    // Clear cache
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
