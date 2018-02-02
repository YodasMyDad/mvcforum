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

    /// <summary>
    ///     A base class for the white site controllers
    /// </summary>
    public class BaseAdminController : Controller
    {
        protected readonly ILocalizationService LocalizationService;
        protected readonly ILoggingService LoggingService;
        protected readonly IMembershipService MembershipService;
        protected readonly ISettingsService SettingsService;
        protected MembershipUser LoggedOnReadOnlyUser;
        protected IMvcForumContext Context;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="loggingService"> </param>
        /// <param name="membershipService"></param>
        /// <param name="localizationService"> </param>
        /// <param name="settingsService"> </param>
        /// <param name="context"></param>
        public BaseAdminController(ILoggingService loggingService,
            IMembershipService membershipService, ILocalizationService localizationService,
            ISettingsService settingsService, IMvcForumContext context)
        {
            MembershipService = membershipService;
            LocalizationService = localizationService;
            LocalizationService.CurrentLanguage = LocalizationService.DefaultLanguage;
            SettingsService = settingsService;
            LoggingService = loggingService;
            Context = context;
            LoggedOnReadOnlyUser = membershipService.GetUser(System.Web.HttpContext.Current.User.Identity.Name, true);
        }

        protected void ShowMessage(GenericMessageViewModel messageViewModel)
        {
            TempData[Constants.MessageViewBagName] = messageViewModel;
        }
    }

    public class UserNotLoggedOnException : Exception
    {
    }
}