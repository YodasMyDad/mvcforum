using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Mvc;
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
    public partial class InstallController : BaseInstallController
    {
        private readonly IInstallerService _installerService;

        private ICategoryService _categoryService;
        private IMembershipService _membershipService;
        private IRoleService _roleService;
        private ILocalizationService _localizationService;
        private ISettingsService _settingsService;
        private IUnitOfWorkManager _unitOfWorkManager;
        private IPermissionService _permissionService;

        public InstallController(IInstallerService installerService)
        {
            _installerService = installerService;
        }

        // This is the default installer
        public ActionResult Index()
        {
            // Quick check of connection string, see if it's close to the original
            // and if so, put a message up asking if they have updated it
            var currentConnectionString = WebConfigurationManager.ConnectionStrings[AppConstants.MvcForumContext].ConnectionString.ToLower();
            if (string.IsNullOrEmpty(currentConnectionString))
            {
                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = @"<p><strong>No Connection String:</strong> You need to update the connection string in the web.config to point to a new blank database</p>",
                    MessageType = GenericMessages.info,
                    ConstantMessage = true
                };
            }


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
                    return RedirectToCreateDb(installerResult, GenericMessages.danger);
                }
            }
            else
            {
                // There was an error creating the database
                return RedirectToCreateDb(installerResult, GenericMessages.danger);
            }


            // Install seems fine
            if (installerResult.Successful)
            {
                // Now we need to update the version in the web.config
                UpdateWebConfigVersionNo(installerResult, currentVersion);

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
            return RedirectToCreateDb(installerResult, GenericMessages.danger);
        }

        public ActionResult UpgradeDb()
        {
            // Work out this install can be upgraded, and if not redirect
            var previousVersionNo = AppHelpers.PreviousVersionNo();
            var currentVersionNo = AppHelpers.GetCurrentVersionNo();

            var installerResult = new InstallerResult{Successful = true, Message = string.Format("Upgrade to v{0} was successful", currentVersionNo)};

            // Can't upgrade so redirect
            if (Convert.ToDouble(previousVersionNo) < 1.3d)
            {
                return RedirectToAction("ManualUpgradeNeeded");
            }

            //***** Old version is v1.3 or greater so we can run the installer ****

            // Firstly add any new tables needed via the SQL
            // Get the SQL file and if it exists then run it
            var dbFilePath = HostingEnvironment.MapPath(InstallerHelper.GetUpdateDatabaseFilePath(currentVersionNo));

            // See whether this version needs a table update
            if (System.IO.File.Exists(dbFilePath))
            {
                // There is a file so update the database with the new tables
                installerResult = _installerService.CreateDbTables(null, dbFilePath, currentVersionNo);
                if (!installerResult.Successful)
                {
                    // Was an error creating the tables
                    return RedirectToCreateDb(installerResult, GenericMessages.danger);
                }
            }

            // Tables created or updated - So now update all the data
            installerResult = UpdateData(currentVersionNo, previousVersionNo, installerResult);

            // See if upgrade was successful or not
            if (installerResult.Successful)
            {
                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = installerResult.OnScreenMessage,
                    MessageType = GenericMessages.success
                };

                // Finally update the web.config to the new version
                UpdateWebConfigVersionNo(installerResult, currentVersionNo);

                return RedirectToAction("Complete");
            }

            return RedirectToCreateDb(installerResult, GenericMessages.danger);
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

        #region Private Methods

        private InstallerResult UpdateData(string currentVersion, string previousVersion, InstallerResult installerResult)
        {
            //Initialise the services
            InitialiseServices();

            // Need to run the updater through all of the below, so we need to do 
            // checks before we add anything to make sure it doesn't already exist and if,
            // update where necessary.
            // Whatever the version update the language strings as these are always the master ones
            // held in the en-GB.csv in the Installer folder root
            installerResult = AddOrUpdateTheDefaultLanguageStrings(installerResult);
            if (!installerResult.Successful)
            {
                return installerResult;
            }

            installerResult.Successful = true;
            installerResult.Message = "All data updated successfully";

            //-----------------------------------------------
            //---------------- v1.3 to v1.4 -----------------
            if (previousVersion.StartsWith("1.3") && currentVersion.StartsWith("1.4"))
            {
                try
                {
                    // No extra data needed in this upgrade


                }
                catch (Exception ex)
                {
                    installerResult.Successful = false;
                    installerResult.Message = string.Format("Error updating from {0} to {1}", previousVersion, currentVersion);
                    installerResult.Exception = ex;
                }
            }
            
            return installerResult;
        }

        private InstallerResult CreateInitialData()
        {
            var installerResult = new InstallerResult { Successful = true, Message = "Congratulations, MVC Forum has installed successfully" };

            // I think this is all I need to call to kick EF into life
            //EFCachingProviderConfiguration.DefaultCache = new AspNetCache();
            //EFCachingProviderConfiguration.DefaultCachingPolicy = CachingPolicy.CacheAll;

            // Now setup the services as we can't do it in the constructor
            InitialiseServices();

            // First UOW to create the data needed for other saves
            using (var unitOfWork = _unitOfWorkManager.NewUnitOfWork())
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
                        var exampleCat = new Category { Name = exampleCatName, ModeratePosts = false, ModerateTopics = false};
                        _categoryService.Add(exampleCat);

                        // Add the default roles
                        var standardRole = new MembershipRole { RoleName = AppConstants.StandardMembers };
                        var guestRole = new MembershipRole { RoleName = AppConstants.GuestRoleName };
                        var adminRole = new MembershipRole { RoleName = AppConstants.AdminRoleName };
                        _roleService.CreateRole(standardRole);
                        _roleService.CreateRole(guestRole);
                        _roleService.CreateRole(adminRole);

                        unitOfWork.Commit();
                    }

                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    installerResult.Exception = ex.InnerException;
                    installerResult.Message = "Error creating the initial data >> Category & Roles";
                    installerResult.Successful = false;
                    return installerResult;
                }
            }

            // Add / Update the default language strings
            installerResult = AddOrUpdateTheDefaultLanguageStrings(installerResult);
            if (!installerResult.Successful)
            {
                return installerResult;
            }   

            // Now we have saved the above we can create the rest of the data
            using (var unitOfWork = _unitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    // if the settings already exist then do nothing
                    if (_settingsService.GetSettings(false) == null)
                    {
                        // Get the default language
                        var startingLanguage = _localizationService.GetLanguageByName("en-GB");

                        // Get the Standard Members role
                        var startingRole = _roleService.GetRole(AppConstants.StandardMembers);

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
                            PrivateMessageFloodControl = 30,
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
                    installerResult.Exception = ex.InnerException;
                    installerResult.Message = "Error creating the initial data >> Settings";
                    installerResult.Successful = false;
                    return installerResult;
                }
            }


            // Now we have saved the above we can create the rest of the data
            using (var unitOfWork = _unitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    // If the admin user exists then don't do anything else
                    if (_membershipService.GetUser("admin") == null)
                    {
                        // Set up the initial category permissions
                        var readOnly = new Permission { Name = AppConstants.PermissionReadOnly };
                        var deletePosts = new Permission { Name = AppConstants.PermissionDeletePosts };
                        var editPosts = new Permission { Name = AppConstants.PermissionEditPosts };
                        var stickyTopics = new Permission { Name = AppConstants.PermissionCreateStickyTopics };
                        var lockTopics = new Permission { Name = AppConstants.PermissionLockTopics };
                        var voteInPolls = new Permission { Name = AppConstants.PermissionVoteInPolls };
                        var createPolls = new Permission { Name = AppConstants.PermissionCreatePolls };
                        var createTopics = new Permission { Name = AppConstants.PermissionCreateTopics };
                        var attachFiles = new Permission { Name = AppConstants.PermissionAttachFiles };
                        var denyAccess = new Permission { Name = AppConstants.PermissionDenyAccess };

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

                        // Set up global permissions
                        var editMembers = new Permission { Name = AppConstants.PermissionEditMembers, IsGlobal = true};

                        _permissionService.Add(editMembers);

                        // create the admin user and put him in the admin role
                        var admin = new MembershipUser
                        {
                            Email = "you@email.com",
                            UserName = "admin",
                            Password = "password",
                            IsApproved = true,
                            DisableEmailNotifications = false,
                            DisablePosting = false,
                            DisablePrivateMessages = false
                        };
                        _membershipService.CreateUser(admin);

                        // Do a save changes just in case
                        unitOfWork.SaveChanges();

                        // Put the admin in the admin role
                        var adminRole = _roleService.GetRole(AppConstants.AdminRoleName);
                        admin.Roles = new List<MembershipRole> { adminRole };

                        unitOfWork.Commit();
                    }
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    installerResult.Exception = ex.InnerException;
                    installerResult.Message = "Error creating the initial data >> Admin user & Permissions";
                    installerResult.Successful = false;
                    return installerResult;
                }
            }

            return installerResult;
        }

        private InstallerResult AddOrUpdateTheDefaultLanguageStrings(InstallerResult installerResult)
        {            
            // Read in CSV and import like it does normally in the admin section
            using (var unitOfWork = _unitOfWorkManager.NewUnitOfWork())
            {
                var report = new CsvReport();

                try
                {
                        // Get the base language file
                    var file = HostingEnvironment.MapPath(@"~/Installer/en-GB.csv");

                        // Verify that the user selected a file
                        if (file != null)
                        {
                            // Unpack the data
                            var allLines = new List<string>();
                            using (var streamReader = new StreamReader(file, Encoding.UTF8, true))
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

                    installerResult.Exception = ex.InnerException;
                    installerResult.Message = "Error creating the initial data >>  Language Strings";
                    if (!string.IsNullOrEmpty(sb.ToString()))
                    {
                        installerResult.Message += string.Concat("<br />", sb.ToString());
                    }
                    installerResult.Successful = false;
                }
                return installerResult; 
            }
        }

        private void InitialiseServices()
        {
            _categoryService = DependencyResolver.Current.GetService<ICategoryService>();
            _membershipService = DependencyResolver.Current.GetService<IMembershipService>();
            _roleService = DependencyResolver.Current.GetService<IRoleService>();
            _localizationService = DependencyResolver.Current.GetService<ILocalizationService>();
            _settingsService = DependencyResolver.Current.GetService<ISettingsService>();
            _unitOfWorkManager = DependencyResolver.Current.GetService<IUnitOfWorkManager>();
            _permissionService = DependencyResolver.Current.GetService<IPermissionService>();
        }

        private void UpdateWebConfigVersionNo(InstallerResult installerResult, string currentVersion)
        {
            if (ConfigUtils.UpdateAppSetting("MVCForumVersion", currentVersion) == false)
            {
                Session[AppConstants.GoToInstaller] = "False";

                installerResult.Message = string.Format(@"Database installed/updated. But there was an error updating the version number in the web.config, you need to manually 
                                                                    update it to {0}", currentVersion);
                installerResult.Successful = false;

                TempData[AppConstants.InstallerName] = AppConstants.InstallerName;

                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = installerResult.OnScreenMessage,
                    MessageType = GenericMessages.danger
                };
            }
        } 
        #endregion
    }
}
