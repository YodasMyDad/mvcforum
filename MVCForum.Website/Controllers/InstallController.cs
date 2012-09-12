using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Web;
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
            if (InstallerHelper.CreateDatabaseTable())
            {
                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = "Congratulations, everything installed correctly you can now login as 'admin' and 'password'",
                    MessageType = GenericMessages.success
                };
                RedirectToAction("Index", "Home");
            }
            TempData[AppConstants.InstallerName] = AppConstants.InstallerName;
            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = "Error creating the database tables, please check the connection string is correct and the database user has the correct permissions",
                MessageType = GenericMessages.error
            };
            return RedirectToAction("CreateDb");
        }

    }
}
