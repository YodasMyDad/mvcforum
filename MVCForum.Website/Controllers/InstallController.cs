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
            if (!ShowInstall()) return RedirectToAction("Index", "Home");

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
            if (!ShowInstall()) return RedirectToAction("Index", "Home");

            TempData[AppConstants.InstallerName] = AppConstants.InstallerName;
            return View();
        }

        /// <summary>
        /// Create the database tables if
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateDbTables()
        {
            TempData[AppConstants.InstallerName] = AppConstants.InstallerName;

            // Check installer should be running
            if (!ShowInstall()) return RedirectToAction("Index", "Home");

            // Get the versions so we can check if its a stright install
            // Or if its an upgrade
            var previousVersion = PreviousVersionNo();
            var currentVersion = GetCurrentVersionNo();

            // Create an installer result so we know everything was successful
            var installerResult = new InstallerResult{Result = false};

            // If blank previous version just install the main database
            if (string.IsNullOrEmpty(previousVersion))
            {
                // Get the file path
                var dbFilePath = InstallerHelper.GetMainDatabaseFilePath(currentVersion);
                installerResult = InstallerHelper.RunSql(dbFilePath);
            }
            else
            {

                var dbFilePath = InstallerHelper.GetUpdateDatabaseFilePath(currentVersion);

                // Not blank so need to work out what to upgrade
                switch (currentVersion)
                {
                    // If 1.2 we are upgrading from 1.1 to 1.2
                    case "1.2":
                        installerResult = InstallerHelper.RunSql(dbFilePath);
                        break;
                }
            }
            
            // Install seems fine
            if (installerResult.Result)
            {
                // Now we need to update the version in the web.config
                if (ConfigUtils.UpdateAppSetting("MVCForumVersion", currentVersion) == false)
                {
                    installerResult.ResultMessage = string.Format(@"Database installed/updated. But there was an error updating the version number in the web.config, you need to manually 
                                                                    update it to {0} and restarting the site",
                                                                    currentVersion);
                }
                else
                {
                    installerResult.ResultMessage = "Congratulations, MVC Forum has installed successfully";
                }

                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = installerResult.ResultMessage,
                    MessageType = GenericMessages.success
                };

                InstallerHelper.TouchWebConfig();
                return RedirectToAction("Complete");
            }

            // If we get here there was an error, so update the UI to tell them
            // If the message is empty then add one
            if (string.IsNullOrEmpty(installerResult.ResultMessage))
            {
                installerResult.ResultMessage = "There was an error during the installer, please try again";
            }

            // Add to temp data and show
            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = installerResult.ResultMessage,
                MessageType = GenericMessages.error
            };

            // Fall back telling user they need a manual upgrade
            return RedirectToAction("CreateDb");
        }

        /// <summary>
        /// Show this if a manual upgrade is needed
        /// </summary>
        /// <returns></returns>
        public ActionResult ManualUpgradeNeeded()
        {
            return View();
        }
        
        /// <summary>
        /// Show this when the installer is complete
        /// </summary>
        /// <returns></returns>
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
            // Store the value for use in the app
            var currentVersionNo = GetCurrentVersionNo();

            // Now check the version in the web.config
            var previousVersionNo = PreviousVersionNo();

            // If the versions are different kick the installer into play
            return (currentVersionNo != previousVersionNo);
        }

        /// <summary>
        /// Get the current version number of the app
        /// </summary>
        /// <returns></returns>
        private static string GetCurrentVersionNo()
        {
            //Installer for new versions and first startup
            // Get the current version
            var version = Assembly.GetExecutingAssembly().GetName().Version;

            // Store the value for use in the app
            return string.Format("{0}.{1}", version.Major, version.Minor);
        }

        /// <summary>
        /// Get the previous version number if there is one from the web.config
        /// </summary>
        /// <returns></returns>
        private static string PreviousVersionNo()
        {
            return ConfigUtils.GetAppSetting("MVCForumVersion");
        }
    }
}
