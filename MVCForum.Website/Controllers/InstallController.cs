using System.Reflection;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Utilities;
using MVCForum.Website.Areas.Admin.ViewModels;
using MVCForum.Website.Installer;

namespace MVCForum.Website.Controllers
{
    public class InstallController : Controller
    {
        // This is the default installer
        public ActionResult Index()
        {
            // Check installer should be running
            if (ShowInstall()) return RedirectToAction("Index", "Home");

            TempData[AppConstants.InstallerName] = AppConstants.InstallerName;
            return View();            
        }

        /// <summary>
        /// Create Db page
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateDb()
        {
            // Check installer should be running
            if (ShowInstall()) return RedirectToAction("Index", "Home");

            TempData[AppConstants.InstallerName] = AppConstants.InstallerName;
            return View();
        }

        /// <summary>
        /// Create the database tables if
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateDbTables()
        {
            // Check installer should be running
            if (ShowInstall()) return RedirectToAction("Index", "Home");

            var installerResponse = InstallerHelper.InstallMainForumTables();
            // and redirect to home page
            if (installerResponse.Result)
            {
                InstallerHelper.TouchWebConfig();
                return RedirectToAction("Complete");
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

        public ActionResult Complete()
        {
            return View();
        }

        /// <summary>
        /// This checks whether the installer should be called, it stops people trying to call the installer
        /// when the application is already installed
        /// </summary>
        /// <returns></returns>
        private static bool ShowInstall()
        {
            //Installer for new versions and first startup
            // Get the current version
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            // Store the value for use in the app
            var versionNo = string.Format("{0}.{1}", version.Major, version.Minor);

            // Now check the version in the web.config
            var currentVersion = ConfigUtils.GetAppSetting("MVCForumVersion");

            // If the versions are different kick the installer into play
            return (currentVersion == versionNo);
        }

    }
}
