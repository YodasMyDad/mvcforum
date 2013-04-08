using System.Web.Mvc;
using System.Web.Routing;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Utilities;
using MVCForum.Website.Application;
using MVCForum.Website.Areas.Admin.ViewModels;
using MVCForum.Website.ViewModels;

namespace MVCForum.Website.Controllers
{
    public class BaseInstallController : Controller
    {
        protected override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // If it gets to the complete action reset the 
            TempData[AppConstants.InstallerName] = AppConstants.InstallerName;

            var actionName = filterContext.ActionDescriptor.ActionName;
            if (actionName.Contains("complete"))
            {
                TempData[AppConstants.InstallerName] = null;
            }

            if (!AppHelpers.ShowInstall())
            {                
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "Home" }, { "action", "Index" } });
                TempData[AppConstants.InstallerName] = null;
            }
        }

        internal ActionResult RedirectToCreateDb(InstallerResult installerResult, GenericMessages status)
        {
            // Add to temp data and show
            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = installerResult.OnScreenMessage,
                MessageType = status
            };

            // Fall back telling user they need a manual upgrade
            return RedirectToAction("CreateDb", "Install");
        }
    }
}