using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.Areas.Admin.ViewModels;

namespace MVCForum.Website.Areas.Admin.Controllers
{
    /// <summary>
    /// A base class for the white site controllers
    /// </summary>
    public partial class BaseAdminController : Controller
    {
        protected readonly IMembershipService MembershipService;
        protected readonly ILocalizationService LocalizationService;
        protected readonly ISettingsService SettingsService;
        protected readonly IUnitOfWorkManager UnitOfWorkManager;
        protected readonly ILoggingService LoggingService;

        protected MembershipUser LoggedOnReadOnlyUser;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="loggingService"> </param>
        /// <param name="unitOfWorkManager"> </param>
        /// <param name="membershipService"></param>
        /// <param name="localizationService"> </param>
        /// <param name="settingsService"> </param>
        public BaseAdminController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, ILocalizationService localizationService, ISettingsService settingsService)
        {
            UnitOfWorkManager = unitOfWorkManager;
            MembershipService = membershipService;
            LocalizationService = localizationService;
            LocalizationService.CurrentLanguage = LocalizationService.DefaultLanguage;
            SettingsService = settingsService;
            LoggingService = loggingService;

            LoggedOnReadOnlyUser = MembershipService.GetUser(System.Web.HttpContext.Current.User.Identity.Name, true);
        }

        protected void ShowMessage(GenericMessageViewModel messageViewModel)
        {
            //ViewData[AppConstants.MessageViewBagName] = messageViewModel;
            TempData[AppConstants.MessageViewBagName] = messageViewModel;
        }

    }

    public class UserNotLoggedOnException : System.Exception
    {
    }
}