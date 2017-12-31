namespace MvcForum.Web.Areas.Admin.Controllers
{
    using System;
    using System.Web.Mvc;
    using Core.Constants;
    using Core.Interfaces.Services;
    using Core.Interfaces.UnitOfWork;
    using Core.Models.Entities;
    using ViewModels;

    /// <summary>
    ///     A base class for the white site controllers
    /// </summary>
    public class BaseAdminController : Controller
    {
        protected readonly ILocalizationService LocalizationService;
        protected readonly ILoggingService LoggingService;
        protected readonly IMembershipService MembershipService;
        protected readonly ISettingsService SettingsService;
        protected readonly IUnitOfWorkManager UnitOfWorkManager;

        protected MembershipUser LoggedOnReadOnlyUser;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="loggingService"> </param>
        /// <param name="unitOfWorkManager"> </param>
        /// <param name="membershipService"></param>
        /// <param name="localizationService"> </param>
        /// <param name="settingsService"> </param>
        public BaseAdminController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager,
            IMembershipService membershipService, ILocalizationService localizationService,
            ISettingsService settingsService)
        {
            UnitOfWorkManager = unitOfWorkManager;
            MembershipService = membershipService;
            LocalizationService = localizationService;
            LocalizationService.CurrentLanguage = LocalizationService.DefaultLanguage;
            SettingsService = settingsService;
            LoggingService = loggingService;

            LoggedOnReadOnlyUser = membershipService.GetUser(System.Web.HttpContext.Current.User.Identity.Name, true);
        }

        protected void ShowMessage(GenericMessageViewModel messageViewModel)
        {
            //ViewData[AppConstants.MessageViewBagName] = messageViewModel;
            TempData[AppConstants.MessageViewBagName] = messageViewModel;
        }
    }

    public class UserNotLoggedOnException : Exception
    {
    }
}