﻿using System.Linq;
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

        protected MembershipUser LoggedOnUser;
        protected MembershipRole UsersRole;

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

            LoggedOnUser = UserIsAuthenticated ? MembershipService.GetUser(Username, true) : null;
            UsersRole = LoggedOnUser == null ? RoleService.GetRole(AppConstants.GuestRoleName, true) : LoggedOnUser.Roles.FirstOrDefault();
        }

        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            var controller = filterContext.RouteData.Values["controller"];
            var area = filterContext.RouteData.DataTokens["area"] ?? string.Empty;

            //if (Session[AppConstants.GoToInstaller] != null && Session[AppConstants.GoToInstaller].ToString() == "True")
            //{
            //    filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "Install" }, { "action", "Index" } });
            //}
            if (SettingsService.GetSettings().IsClosed && !filterContext.IsChildAction)
            {
                // Only redirect if its closed and user is NOT in the admin
                if (controller.ToString().ToLower() != "closed" && controller.ToString().ToLower() != "members" && !area.ToString().ToLower().Contains("admin"))
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

        protected bool UserIsAdmin
        {
            get { return User.IsInRole(AppConstants.AdminRoleName); }
        }

        protected void ShowMessage(GenericMessageViewModel messageViewModel)
        {
            //ViewData[AppConstants.MessageViewBagName] = messageViewModel;
            TempData[AppConstants.MessageViewBagName] = messageViewModel;
        }
        protected string Username
        {
            get
            {
                return UserIsAuthenticated ? System.Web.HttpContext.Current.User.Identity.Name : null;
            }
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

    public class UserNotLoggedOnException : System.Exception
    {

    }
}