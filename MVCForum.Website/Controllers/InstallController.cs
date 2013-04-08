using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using EFCachingProvider;
using EFCachingProvider.Caching;
using EFCachingProvider.Web;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Utilities;
using MVCForum.Website.Application;
using MVCForum.Website.Areas.Admin.ViewModels;
using MVCForum.Website.ViewModels;

namespace MVCForum.Website.Controllers
{
    public class InstallController : BaseInstallController
    {
        private readonly IInstallerService _installerService;

        private ICategoryService _categoryService;
        private IMembershipService _membershipService;
        private IRoleService _roleService;
        private ILocalizationService _localizationService;
        private ISettingsService _settingsService;
        private IUnitOfWorkManager _UnitOfWorkManager;
        private IPermissionService _permissionService;

        public InstallController(IInstallerService installerService)
        {
            _installerService = installerService;
        }

        // This is the default installer
        public ActionResult Index()
        {
            return View();
        }

        /// <summary>
        /// Create Db page
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateDb()
        {
            // Check installer should be running
            var previousVersionNo = AppHelpers.PreviousVersionNo();
            var viewModel = new CreateDbViewModel
                {
                    IsUpgrade = !string.IsNullOrEmpty(previousVersionNo),
                    PreviousVersion = previousVersionNo,
                    CurrentVersion = AppHelpers.GetCurrentVersionNo()
                };

            return View(viewModel);
        }

        /// <summary>
        /// Create the database tables if
        /// </summary>
        /// <returns></returns>
        public ActionResult CreateDbTables()
        {
            // Get the versions so we can check if its a stright install
            // Or if its an upgrade
            var currentVersion = AppHelpers.GetCurrentVersionNo();

            // Create an installer result so we know everything was successful
            var installerResult = new InstallerResult { Successful = true, Message = "Congratulations, MVC Forum has installed successfully" };

            // Create the main database tables
            // NOTE: For testing you can overide the connectionstring and filepath to the sql
            // Just replace the nulls below
            installerResult = _installerService.CreateDbTables(null, null, currentVersion);

            if (installerResult.Successful)
            {
                // Create the base data
                //installerResult = _installerService.CreateInitialData();
                installerResult = CreateInitialData();

                // If error creating the base data then return the error
                if (!installerResult.Successful)
                {
                    // There was an error creating the database
                    return RedirectToCreateDb(installerResult, GenericMessages.error);
                }
            }
            else
            {
                // There was an error creating the database
                return RedirectToCreateDb(installerResult, GenericMessages.error);
            }


            // Install seems fine
            if (installerResult.Successful)
            {
                // Now we need to update the version in the web.config
                if (ConfigUtils.UpdateAppSetting("MVCForumVersion", currentVersion) == false)
                {
                    installerResult.Message = string.Format(@"Database installed/updated. But there was an error updating the version number in the web.config, you need to manually 
                                                                    update it to {0}", currentVersion);
                    installerResult.Successful = false;

                    TempData[AppConstants.InstallerName] = AppConstants.InstallerName;

                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = installerResult.OnScreenMessage,
                        MessageType = GenericMessages.error
                    };
                }

                // This code will never be hit as the update to the web.config above will trigger the app restart and 
                // it will find the version number and redircet them to the home page - Only way its hit is if the update doesn't work
                return RedirectToAction("Complete");
            }

            // If we get here there was an error, so update the UI to tell them
            // If the message is empty then add one
            if (string.IsNullOrEmpty(installerResult.Message))
            {
                installerResult.Message = @"There was an unkown error during the installer, please try again. If the problem continues 
                                                    then please let us know <a target='_blank' href='http://chat.mvcforum.com'>on the support forums</a>";
            }

            // Add to temp data and show
            return RedirectToCreateDb(installerResult, GenericMessages.error);
        }

        private InstallerResult CreateInitialData()
        {
            var installerResult = new InstallerResult { Successful = true, Message = "Congratulations, MVC Forum has installed successfully" };

            // I think this is all I need to call to kick EF into life
            //EFCachingProviderConfiguration.DefaultCache = new AspNetCache();
            //EFCachingProviderConfiguration.DefaultCachingPolicy = CachingPolicy.CacheAll;

            // Now setup the services as we can't do it in the constructor
            _categoryService = DependencyResolver.Current.GetService<ICategoryService>();
            _membershipService = DependencyResolver.Current.GetService<IMembershipService>();
            _roleService = DependencyResolver.Current.GetService<IRoleService>();
            _localizationService = DependencyResolver.Current.GetService<ILocalizationService>();
            _settingsService = DependencyResolver.Current.GetService<ISettingsService>();
            _UnitOfWorkManager = DependencyResolver.Current.GetService<IUnitOfWorkManager>();
            _permissionService = DependencyResolver.Current.GetService<IPermissionService>();

            // First UOW to create the data needed for other saves
            using (var unitOfWork = _UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    // Check if category exists or not, we only do a single check for the first object within this
                    // UOW because, if anything failed inside. Everything else would be rolled back to because of the 
                    // transaction
                    const string exampleCatName = "Example Category";
                    if (_categoryService.GetAll().FirstOrDefault(x => x.Name == exampleCatName) == null)
                    {
                        // Doesn't exist so add the example category
                        var exampleCat = new Category { Name = exampleCatName };
                        _categoryService.Add(exampleCat);

                        // Add the default roles
                        var standardRole = new MembershipRole { RoleName = "Standard Members" };
                        var guestRole = new MembershipRole { RoleName = "Guest" };
                        var moderatorRole = new MembershipRole { RoleName = "Moderator" };
                        var adminRole = new MembershipRole { RoleName = "Admin" };
                        _roleService.CreateRole(standardRole);
                        _roleService.CreateRole(guestRole);
                        _roleService.CreateRole(moderatorRole);
                        _roleService.CreateRole(adminRole);

                        unitOfWork.Commit();
                    }

                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    installerResult.Exception = ex;
                    installerResult.Message = "Error creating the initial data >> Category & Roles";
                    installerResult.Successful = false;
                    return installerResult;
                }
            }

            using (var unitOfWork = _UnitOfWorkManager.NewUnitOfWork())
            {
                // Read in CSV and import like it does normally in the admin section
                var report = new CsvReport();

                try
                {
                    // If there is already a language then it must have been successful 
                    // so no need to do anything
                    if (!_localizationService.AllLanguages.Any())
                    {
                        // Get the base language file
                        var file = System.Web.HttpContext.Current.Server.MapPath(@"~/Installer/en-GB.csv");

                        // Verify that the user selected a file
                        if (file != null)
                        {
                            // Unpack the data
                            var allLines = new List<string>();
                            using (var streamReader = new StreamReader(file, Encoding.Unicode, true))
                            {
                                while (streamReader.Peek() >= 0)
                                {
                                    allLines.Add(streamReader.ReadLine());
                                }
                            }

                            // Read the CSV file and generate a language
                            report = _localizationService.FromCsv("en-GB", allLines);
                        }

                        unitOfWork.Commit();
                    }
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();

                    //Loop through report errors and spit them out in the installer result message?
                    var sb = new StringBuilder();
                    foreach (var error in report.Errors)
                    {
                        if (error.ErrorWarningType == CsvErrorWarningType.BadDataFormat ||
                            error.ErrorWarningType == CsvErrorWarningType.GeneralError)
                        {
                            sb.AppendFormat("{0}<br />", error.Message);
                        }
                    }

                    installerResult.Exception = ex;
                    installerResult.Message = "Error creating the initial data >>  Language Strings";
                    if (!string.IsNullOrEmpty(sb.ToString()))
                    {
                        installerResult.Message += string.Concat("<br />", sb.ToString());
                    }
                    installerResult.Successful = false;
                    return installerResult;
                }
            }

            // Now we have saved the above we can create the rest of the data
            using (var unitOfWork = _UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    // if the settings already exist then do nothing
                    if (_settingsService.GetSettings(false) == null)
                    {
                        // Get the default language
                        var startingLanguage = _localizationService.GetLanguageByName("en-GB");

                        // Get the Standard Members role
                        var startingRole = _roleService.GetRole("Standard Members");

                        // create the settings
                        var settings = new Settings
                            {
                                ForumName = "MVC Forum",
                                ForumUrl = "http://www.mydomain.com",
                                IsClosed = false,
                                EnableRSSFeeds = true,
                                DisplayEditedBy = true,
                                EnablePostFileAttachments = false,
                                EnableMarkAsSolution = true,
                                EnableSpamReporting = true,
                                EnableMemberReporting = true,
                                EnableEmailSubscriptions = true,
                                ManuallyAuthoriseNewMembers = false,
                                EmailAdminOnNewMemberSignUp = true,
                                TopicsPerPage = 20,
                                PostsPerPage = 20,
                                EnablePrivateMessages = true,
                                MaxPrivateMessagesPerMember = 50,
                                PrivateMessageFloodControl = 1,
                                EnableSignatures = false,
                                EnablePoints = true,
                                PointsAllowedToVoteAmount = 1,
                                PointsAddedPerPost = 1,
                                PointsAddedForSolution = 4,
                                PointsDeductedNagativeVote = 2,
                                AdminEmailAddress = "my@email.com",
                                NotificationReplyEmail = "noreply@myemail.com",
                                SMTPEnableSSL = false,
                                Theme = "Metro",
                                NewMemberStartingRole = startingRole,
                                DefaultLanguage = startingLanguage,
                                ActivitiesPerPage = 20,
                                EnableAkisment = false,
                                EnableSocialLogins = false,
                                EnablePolls = true
                            };
                        _settingsService.Add(settings);

                        unitOfWork.Commit();
                    }
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    installerResult.Exception = ex;
                    installerResult.Message = "Error creating the initial data >> Settings";
                    installerResult.Successful = false;
                    return installerResult;
                }
            }


            // Now we have saved the above we can create the rest of the data
            using (var unitOfWork = _UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    // If the admin user exists then don't do anything else
                    if (_membershipService.GetUser("admin") == null)
                    {
                        // Set up the initial permissions
                        var readOnly = new Permission { Name = "Read Only" };
                        var deletePosts = new Permission { Name = "Delete Posts" };
                        var editPosts = new Permission { Name = "Edit Posts" };
                        var stickyTopics = new Permission { Name = "Sticky Topics" };
                        var lockTopics = new Permission { Name = "Lock Topics" };
                        var voteInPolls = new Permission { Name = "Vote In Polls" };
                        var createPolls = new Permission { Name = "Create Polls" };
                        var createTopics = new Permission { Name = "Create Topics" };
                        var attachFiles = new Permission { Name = "Attach Files" };
                        var denyAccess = new Permission { Name = "Deny Access" };

                        _permissionService.Add(readOnly);
                        _permissionService.Add(deletePosts);
                        _permissionService.Add(editPosts);
                        _permissionService.Add(stickyTopics);
                        _permissionService.Add(lockTopics);
                        _permissionService.Add(voteInPolls);
                        _permissionService.Add(createPolls);
                        _permissionService.Add(createTopics);
                        _permissionService.Add(attachFiles);
                        _permissionService.Add(denyAccess);

                        // create the admin user and put him in the admin role
                        var admin = new MembershipUser
                        {
                            Email = "you@email.com",
                            UserName = "admin",
                            Password = "password",
                            IsApproved = true
                        };
                        _membershipService.CreateUser(admin);

                        // Do a save changes just in case
                        unitOfWork.SaveChanges();

                        // Put the admin in the admin role
                        var adminRole = _roleService.GetRole("Admin");
                        admin.Roles = new List<MembershipRole> { adminRole };

                        unitOfWork.Commit(); 
                    }
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    installerResult.Exception = ex;
                    installerResult.Message = "Error creating the initial data >> Admin user & Permissions";
                    installerResult.Successful = false;
                    return installerResult;
                }
            }

            return installerResult;
        }

        public ActionResult UpgradeDb()
        {
            throw new NotImplementedException();

            // I think this is all I need to call to kick EF into life
            EFCachingProviderConfiguration.DefaultCache = new AspNetCache();
            EFCachingProviderConfiguration.DefaultCachingPolicy = CachingPolicy.CacheAll;

            // OLD ORIGINAL CODE
            //var dbFilePath = InstallerHelper.GetUpdateDatabaseFilePath(currentVersion);

            //// Not blank so need to work out what to upgrade
            //switch (currentVersion)
            //{
            //    // If 1.2 we are upgrading from 1.1 to 1.2
            //    case "1.2":
            //        installerResult = InstallerHelper.RunSql(dbFilePath);
            //        break;
            //}

            //return View();
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



    }
}
