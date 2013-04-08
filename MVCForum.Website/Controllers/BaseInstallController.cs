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
            TempData[AppConstants.InstallerName] = AppConstants.InstallerName;

            if (!AppHelpers.ShowInstall())
            {                
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary { { "controller", "Home" }, { "action", "Index" } });
            }
        }

        internal ActionResult RedirectToCreateDb(InstallerResult installerResult, GenericMessages status)
        {
            // Add to temp data and show
            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = installerResult.Message,
                MessageType = status
            };

            // Fall back telling user they need a manual upgrade
            return RedirectToAction("CreateDb", "Install");
        }
    }
}