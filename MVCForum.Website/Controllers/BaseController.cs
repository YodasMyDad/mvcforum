using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.Areas.Admin.ViewModels;

namespace MVCForum.Website.Controllers
{
    /// <summary>
    /// A base class for the white site controllers
    /// </summary>
    public class BaseController : Controller
    {
        protected readonly IUnitOfWorkManager UnitOfWorkManager;
        protected readonly IMembershipService MembershipService;
        protected readonly ILocalizationService LocalizationService;
        protected readonly IRoleService RoleService;
        protected readonly ISettingsService SettingsService;
        protected readonly ILoggingService LoggingService;

        //private readonly MembershipUser _loggedInUser;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="loggingService"> </param>
        /// <param name="unitOfWorkManager"> </param>
        /// <param name="membershipService"></param>
        /// <param name="localizationService"> </param>
        /// <param name="roleService"> </param>
        /// <param name="settingsService"> </param>
        public BaseController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService)
        {
            UnitOfWorkManager = unitOfWorkManager;
            MembershipService = membershipService;
            LocalizationService = localizationService;
            RoleService = roleService;
            SettingsService = settingsService;
            LoggingService = loggingService;
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.RouteData.Values["controller"];

            //if (Session[AppConstants.GoToInstaller] != null && Session[AppConstants.GoToInstaller].ToString() == "True")
            //{
            //    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "Install" }, { "action", "Index" } });
            //}
            if (SettingsService.GetSettings().IsClosed)
            {                
                if(controller.ToString().ToLower() != "closed")
                {
                    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "Closed" }, { "action", "Index" } });
                }          
            }
        }

        protected bool UserIsAuthenticated
        {
            get
            {
                return System.Web.HttpContext.Current.User.Identity.IsAuthenticated;
            }
        }

        protected string Username
        {
            get
            {
                return UserIsAuthenticated ? System.Web.HttpContext.Current.User.Identity.Name : null;
            }
        }

        ///// <summary>
        ///// Return the currently logged on user
        ///// </summary>
        //protected MembershipUser LoggedOnUser
        //{
        //    get
        //    {
        //        if (User.Identity.IsAuthenticated)
        //        {
        //            //TODO: THIS GETS HIT LOADS OF TIMES!! NEED TO MOVE / REFACTOR
        //            var currentUser = MembershipService.GetUser(User.Identity.Name);
        //            return currentUser;
        //        }
        //        return null;
        //    }
        //}

        //protected MembershipRole UsersRole
        //{
        //    get
        //    {
        //            return LoggedOnUser == null ? RoleService.GetRole(AppConstants.GuestRoleName) : LoggedOnUser.Roles.FirstOrDefault(); 
        //    }
        //}

        internal ActionResult ErrorToHomePage(string errorMessage)
        {
            // Use temp data as its a redirect
            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = errorMessage,
                MessageType = GenericMessages.error
            };
            // Not allowed in here so
            return RedirectToAction("Index", "Home");
        }
    }

    public class UserNotLoggedOnException : System.Exception
    {

    }
}