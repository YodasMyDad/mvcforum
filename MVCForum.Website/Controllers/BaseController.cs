namespace MvcForum.Web.Controllers
{
    using System.Globalization;
    using System.Threading;
    using System.Web.Mvc;
    using System.Web.Routing;
    using System.Web.Security;
    using Core.Constants;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Core.Utilities;
    using ViewModels;

    /// <summary>
    ///     A base class for the white site controllers
    /// </summary>
    public partial class BaseController : Controller
    {
        protected readonly ICacheService CacheService;
        protected readonly ILocalizationService LocalizationService;
        protected readonly ILoggingService LoggingService;
        protected readonly IMembershipService MembershipService;
        protected readonly IRoleService RoleService;
        protected readonly ISettingsService SettingsService;
        protected readonly IMvcForumContext Context;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="loggingService"> </param>
        /// <param name="membershipService"></param>
        /// <param name="localizationService"> </param>
        /// <param name="roleService"> </param>
        /// <param name="settingsService"> </param>
        /// <param name="cacheService"></param>
        /// <param name="context"></param>
        public BaseController(ILoggingService loggingService,
            IMembershipService membershipService, ILocalizationService localizationService, IRoleService roleService,
            ISettingsService settingsService, ICacheService cacheService, IMvcForumContext context)
        {
            MembershipService = membershipService;
            LocalizationService = localizationService;
            RoleService = roleService;
            SettingsService = settingsService;
            CacheService = cacheService;
            Context = context;
            LoggingService = loggingService;
        }

        protected override void Initialize(RequestContext requestContext)
        {
            base.Initialize(requestContext);

            if (Session != null)
            {
                // Set the culture per request
                var ci = new CultureInfo(LocalizationService.CurrentLanguage.LanguageCulture);
                Thread.CurrentThread.CurrentUICulture = ci;
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(ci.Name);
            }
        }

        protected string Domain => StringUtils.ReturnCurrentDomain();

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


            var loggedOnReadOnlyUser = User.Identity.IsAuthenticated
                ? MembershipService.GetUser(User.Identity.Name, true)
                : null;
     

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
                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
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
                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
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
            //ViewData[Constants.MessageViewBagName] = messageViewModel;
            TempData[Constants.MessageViewBagName] = messageViewModel;
        }

        internal ActionResult ErrorToHomePage(string errorMessage)
        {
            // Use temp data as its a redirect
            TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = errorMessage,
                MessageType = GenericMessages.danger
            };
            // Not allowed in here so
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        ///     The on exception.
        /// </summary>
        /// <param name="filterContext">
        ///     The filter context.
        /// </param>
        protected override void OnException(ExceptionContext filterContext)
        {
            LoggingService.Error(filterContext.Exception);
        }
    }
}