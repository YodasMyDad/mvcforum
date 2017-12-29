namespace MvcForum.Web.Controllers
{
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.Security;
    using Areas.Admin.ViewModels;
    using Core.Constants;
    using Core.Interfaces.Services;
    using Core.Interfaces.UnitOfWork;
    using Core.Utilities;
    using MembershipUser = Core.DomainModel.Entities.MembershipUser;

    /// <summary>
    ///     A base class for the white site controllers
    /// </summary>
    public class BaseController : Controller
    {
        protected readonly ICacheService CacheService;
        protected readonly ILocalizationService LocalizationService;
        protected readonly ILoggingService LoggingService;
        protected readonly IMembershipService MembershipService;
        protected readonly IRoleService RoleService;
        protected readonly ISettingsService SettingsService;
        protected readonly IUnitOfWorkManager UnitOfWorkManager;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="loggingService"> </param>
        /// <param name="unitOfWorkManager"> </param>
        /// <param name="membershipService"></param>
        /// <param name="localizationService"> </param>
        /// <param name="roleService"> </param>
        /// <param name="settingsService"> </param>
        /// <param name="cacheService"></param>
        public BaseController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager,
            IMembershipService membershipService, ILocalizationService localizationService, IRoleService roleService,
            ISettingsService settingsService, ICacheService cacheService)
        {
            UnitOfWorkManager = unitOfWorkManager;
            MembershipService = membershipService;
            LocalizationService = localizationService;
            RoleService = roleService;
            SettingsService = settingsService;
            CacheService = cacheService;
            LoggingService = loggingService;
        }

        protected string Domain => CacheService.CachePerRequest(CacheKeys.Domain, StringUtils.ReturnCurrentDomain);

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.RouteData.Values["controller"];
            var action = filterContext.RouteData.Values["action"];
            var area = filterContext.RouteData.DataTokens["area"] ?? string.Empty;
            var settings = SettingsService.GetSettings();

            // Check if forum is closed
            if (settings.IsClosed && !filterContext.IsChildAction)
            {
                // Only redirect if its closed and user is NOT in the admin
                if (controller.ToString().ToLower() != "closed" && controller.ToString().ToLower() != "members" &&
                    !area.ToString().ToLower().Contains("admin"))
                {
                    filterContext.Result =
                        new RedirectToRouteResult(new RouteValueDictionary
                        {
                            {"controller", "Closed"},
                            {"action", "Index"}
                        });
                }
            }

            MembershipUser loggedOnReadOnlyUser;

            using (UnitOfWorkManager.NewUnitOfWork())
            {
                loggedOnReadOnlyUser = User.Identity.IsAuthenticated
                    ? MembershipService.GetUser(User.Identity.Name, true)
                    : null;
            }

            // Check if they need to agree to permissions
            if (SettingsService.GetSettings().AgreeToTermsAndConditions == true && !filterContext.IsChildAction &&
                loggedOnReadOnlyUser != null && loggedOnReadOnlyUser.HasAgreedToTermsAndConditions != true)
            {
                // Only redirect if its closed and user is NOT in the admin
                if (action.ToString().ToLower() != "termsandconditions" && !area.ToString().ToLower().Contains("admin"))
                {
                    filterContext.Result =
                        new RedirectToRouteResult(new RouteValueDictionary
                        {
                            {"controller", "Home"},
                            {"action", "TermsAndConditions"}
                        });
                }
            }

            // If the forum is new members need approving and the user is not approved, log them out
            if (loggedOnReadOnlyUser != null && !loggedOnReadOnlyUser.IsApproved &&
                settings.NewMemberEmailConfirmation == true)
            {
                FormsAuthentication.SignOut();
                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Members.MemberEmailAuthorisationNeeded"),
                    MessageType = GenericMessages.success
                };
                filterContext.Result =
                    new RedirectToRouteResult(new RouteValueDictionary {{"controller", "Home"}, {"action", "Index"}});
            }

            // If the user is banned - Log them out.
            if (loggedOnReadOnlyUser != null && loggedOnReadOnlyUser.IsBanned)
            {
                FormsAuthentication.SignOut();
                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Members.NowBanned"),
                    MessageType = GenericMessages.danger
                };
                filterContext.Result =
                    new RedirectToRouteResult(new RouteValueDictionary {{"controller", "Home"}, {"action", "Index"}});
            }
        }

        protected void ShowMessage(GenericMessageViewModel messageViewModel)
        {
            //ViewData[AppConstants.MessageViewBagName] = messageViewModel;
            TempData[AppConstants.MessageViewBagName] = messageViewModel;
        }

        internal ActionResult ErrorToHomePage(string errorMessage)
        {
            // Use temp data as its a redirect
            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = errorMessage,
                MessageType = GenericMessages.danger
            };
            // Not allowed in here so
            return RedirectToAction("Index", "Home");
        }
    }
}