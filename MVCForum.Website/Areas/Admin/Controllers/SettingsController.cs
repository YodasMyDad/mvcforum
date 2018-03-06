namespace MvcForum.Web.Areas.Admin.Controllers
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;
    using Application;
    using Core.Constants;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Core.Models;
    using Core.Models.Entities;
    using Web.ViewModels;
    using Web.ViewModels.Admin;
    using Web.ViewModels.Mapping;

    [Authorize(Roles = Constants.AdminRoleName)]
    public class SettingsController : BaseAdminController
    {
        private readonly ICacheService _cacheService;
        private readonly IEmailService _emailService;
        private readonly IRoleService _roleService;

        public SettingsController(ILoggingService loggingService, ILocalizationService localizationService,
            IMembershipService membershipService, IRoleService roleService, ISettingsService settingsService,
            IEmailService emailService, ICacheService cacheService, IMvcForumContext context)
            : base(loggingService, membershipService, localizationService, settingsService, context)
        {
            _roleService = roleService;
            _emailService = emailService;
            _cacheService = cacheService;
        }

        public ActionResult Index()
        {
            var currentSettings = SettingsService.GetSettings();
            var settingViewModel = ViewModelMapping.SettingsToSettingsViewModel(currentSettings);
            settingViewModel.NewMemberStartingRole =
                _roleService.GetRole(currentSettings.NewMemberStartingRole.Id).Id;
            settingViewModel.DefaultLanguage = LocalizationService.DefaultLanguage.Id;
            settingViewModel.Roles = _roleService.AllRoles().ToList();
            settingViewModel.Languages = LocalizationService.AllLanguages.ToList();
            return View(settingViewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Index(EditSettingsViewModel settingsViewModel)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var existingSettings = SettingsService.GetSettings(false);
                    var updatedSettings =
                        ViewModelMapping.SettingsViewModelToSettings(settingsViewModel, existingSettings);

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

                    Context.SaveChanges();
                    _cacheService.ClearStartsWith(CacheKeys.Settings.Main);
                }
                catch (Exception ex)
                {
                    Context.RollBack();
                    LoggingService.Error(ex);
                }


                // All good clear cache and get reliant lists

                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = "Settings Updated",
                    MessageType = GenericMessages.success
                };
                settingsViewModel.Themes = AppHelpers.GetThemeFolders();
                settingsViewModel.Roles = _roleService.AllRoles().ToList();
                settingsViewModel.Languages = LocalizationService.AllLanguages.ToList();
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
                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
                message.Message = "Error sending email";
                message.MessageType = GenericMessages.danger;
            }
            TempData[Constants.MessageViewBagName] = message;

            return RedirectToAction("Index");
        }

        public ActionResult CustomCode()
        {
            var settings = SettingsService.GetSettings();
            var viewModel = new CustomCodeViewModels
            {
                CustomFooterCode = settings.CustomFooterCode,
                CustomHeaderCode = settings.CustomHeaderCode
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult CustomCode(CustomCodeViewModels viewModel)
        {
            var settings = SettingsService.GetSettings(false);

            settings.CustomFooterCode = viewModel.CustomFooterCode;
            settings.CustomHeaderCode = viewModel.CustomHeaderCode;

            try
            {
                Context.SaveChanges();

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