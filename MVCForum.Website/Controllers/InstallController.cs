using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Website.Areas.Admin.ViewModels;
using MVCForum.Website.Installer;

namespace MVCForum.Website.Controllers
{
    public class InstallController : Controller
    {
        // This is the default installer
        public ActionResult Index()
        {
            TempData[AppConstants.InstallerName] = AppConstants.InstallerName;
            return View();            
        }

        /// <summary>
        /// Create Db page
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateDb()
        {
            TempData[AppConstants.InstallerName] = AppConstants.InstallerName;
            return View();
        }

        /// <summary>
        /// Create the database tables if
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateDbTables()
        {
            var installerResponse = InstallerHelper.InstallMainForumTables();
            // and redirect to home page
            if (installerResponse.Result)
            {
                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = installerResponse.ResultMessage,
                    MessageType = GenericMessages.success
                };
                Response.Redirect("/");
            }

            // If we get here there was an error, so update the UI to tell them
            TempData[AppConstants.InstallerName] = AppConstants.InstallerName;
            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = installerResponse.ResultMessage,
                MessageType = GenericMessages.error
            };
            return RedirectToAction("Index");
        }

    }
}
