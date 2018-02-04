namespace MvcForum.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Security.Principal;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using System.Web.Security;
    using Core;
    using Core.Constants;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Interfaces.Services;
    using Core.Models;
    using Core.Models.Enums;
    using Core.Models.General;
    using Core.Pipeline;
    using Core.Reflection;
    using ViewModels;
    using ViewModels.Admin;
    using ViewModels.ExtensionMethods;
    using ViewModels.Mapping;
    using ViewModels.Member;
    using ViewModels.Registration;
    using MembershipUser = Core.Models.Entities.MembershipUser;

    /// <summary>
    ///     Members controller
    /// </summary>
    public partial class MembersController : BaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly IEmailService _emailService;
        private readonly IFavouriteService _favouriteService;
        private readonly INotificationService _notificationService;
        private readonly IPollService _pollService;
        private readonly IPostService _postService;
        private readonly IPrivateMessageService _privateMessageService;
        private readonly IReportService _reportService;
        private readonly ITopicService _topicService;
        private readonly IVoteService _voteService;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="loggingService"></param>
        /// <param name="membershipService"></param>
        /// <param name="localizationService"></param>
        /// <param name="roleService"></param>
        /// <param name="settingsService"></param>
        /// <param name="postService"></param>
        /// <param name="reportService"></param>
        /// <param name="emailService"></param>
        /// <param name="privateMessageService"></param>
        /// <param name="categoryService"></param>
        /// <param name="topicService"></param>
        /// <param name="cacheService"></param>
        /// <param name="notificationService"></param>
        /// <param name="pollService"></param>
        /// <param name="voteService"></param>
        /// <param name="favouriteService"></param>
        /// <param name="context"></param>
        public MembersController(ILoggingService loggingService, IMembershipService membershipService,
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService,
            IPostService postService, IReportService reportService, IEmailService emailService,
            IPrivateMessageService privateMessageService, ICategoryService categoryService, ITopicService topicService,
            ICacheService cacheService, INotificationService notificationService,
            IPollService pollService, IVoteService voteService, IFavouriteService favouriteService,
            IMvcForumContext context)
            : base(loggingService, membershipService, localizationService, roleService,
                settingsService, cacheService, context)
        {
            _postService = postService;
            _reportService = reportService;
            _emailService = emailService;
            _privateMessageService = privateMessageService;
            _categoryService = categoryService;
            _topicService = topicService;
            _notificationService = notificationService;
            _pollService = pollService;
            _voteService = voteService;
            _favouriteService = favouriteService;
        }

        /// <summary>
        ///     Scrubs a user and bans them
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = Constants.AdminRoleName)]
        public virtual async Task<ActionResult> SrubAndBanUser(Guid id)
        {
            var user = MembershipService.GetUser(id);
            var scrubResult = await MembershipService.ScrubUsers(user);
            if (!scrubResult.Successful)
            {
                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Members.UnSuccessfulSrub"),
                    MessageType = GenericMessages.danger
                };
            }
            else
            {
                // Set the user to banned
                scrubResult.EntityToProcess.IsBanned = true;

                // Save
                await Context.SaveChangesAsync();

                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Members.SuccessfulSrub"),
                    MessageType = GenericMessages.success
                };
            }
            var viewModel = ViewModelMapping.UserToMemberEditViewModel(user);
            viewModel.AllRoles = RoleService.AllRoles();
            return Redirect(user.NiceUrl);
        }

        /// <summary>
        ///     Ban a member
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public virtual ActionResult BanMember(Guid id)
        {
            var user = MembershipService.GetUser(id);
            var currentUser = MembershipService.GetUser(User.Identity.Name, true);
            var permissions = RoleService.GetPermissions(null, currentUser.Roles.FirstOrDefault());

            if (permissions[ForumConfiguration.Instance.PermissionEditMembers].IsTicked)
            {
                if (!user.Roles.Any(x => x.RoleName.Contains(Constants.AdminRoleName)))
                {
                    user.IsBanned = true;

                    try
                    {
                        Context.SaveChanges();
                        TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = LocalizationService.GetResourceString("Members.NowBanned"),
                            MessageType = GenericMessages.success
                        };
                    }
                    catch (Exception ex)
                    {
                        Context.RollBack();
                        LoggingService.Error(ex);
                        TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = LocalizationService.GetResourceString("Error.UnableToBanMember"),
                            MessageType = GenericMessages.danger
                        };
                    }
                }
            }

            return Redirect(user.NiceUrl);
        }

        /// <summary>
        ///     Unban a member
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public virtual ActionResult UnBanMember(Guid id)
        {
            var user = MembershipService.GetUser(id);
            var currentUser = MembershipService.GetUser(User.Identity.Name, true);
            var permissions = RoleService.GetPermissions(null, currentUser.Roles.FirstOrDefault());

            if (permissions[ForumConfiguration.Instance.PermissionEditMembers].IsTicked)
            {
                if (!user.Roles.Any(x => x.RoleName.Contains(Constants.AdminRoleName)))
                {
                    user.IsBanned = false;

                    try
                    {
                        Context.SaveChanges();
                        TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = LocalizationService.GetResourceString("Members.NowUnBanned"),
                            MessageType = GenericMessages.success
                        };
                    }
                    catch (Exception ex)
                    {
                        Context.RollBack();
                        LoggingService.Error(ex);
                        TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = LocalizationService.GetResourceString("Error.UnableToUnBanMember"),
                            MessageType = GenericMessages.danger
                        };
                    }
                }
            }

            return Redirect(user.NiceUrl);
        }

        /// <summary>
        ///     Show current active members
        /// </summary>
        /// <returns></returns>
        [ChildActionOnly]
        public virtual PartialViewResult GetCurrentActiveMembers()
        {
            var viewModel = new ActiveMembersViewModel
            {
                ActiveMembers = MembershipService.GetActiveMembers()
            };
            return PartialView(viewModel);
        }

        /// <summary>
        ///     Does a last active check
        /// </summary>
        /// <returns></returns>
        public virtual JsonResult LastActiveCheck()
        {
            if (User.Identity.IsAuthenticated)
            {
                var rightNow = DateTime.UtcNow;
                var currentUser = MembershipService.GetUser(User.Identity.Name, true);
                var usersDate = currentUser.LastActivityDate ?? DateTime.UtcNow.AddDays(-1);

                var span = rightNow.Subtract(usersDate);
                var totalMins = span.TotalMinutes;

                if (totalMins > Constants.TimeSpanInMinutesToDoCheck)
                {
                    // Actually get the user, LoggedOnUser has no tracking
                    var loggedOnUser = MembershipService.GetUser(User.Identity.Name);

                    // Update users last activity date so we can show the latest users online
                    loggedOnUser.LastActivityDate = DateTime.UtcNow;

                    // Update
                    try
                    {
                        Context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        Context.RollBack();
                        LoggingService.Error(ex);
                    }
                }
            }

            // You can return anything to reset the timer.
            return Json(new { Timer = "reset" }, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Gets a member by slug
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public virtual ActionResult GetByName(string slug)
        {
            var member = MembershipService.GetUserBySlug(slug);
            var loggedOnReadOnlyUser = User.Identity.IsAuthenticated
                ? MembershipService.GetUser(User.Identity.Name, true)
                : null;
            var usersRole = loggedOnReadOnlyUser == null
                ? RoleService.GetRole(Constants.GuestRoleName, true)
                : loggedOnReadOnlyUser.Roles.FirstOrDefault();
            var loggedonId = loggedOnReadOnlyUser?.Id ?? Guid.Empty;
            var permissions = RoleService.GetPermissions(null, usersRole);

            // Localise the badge names
            foreach (var item in member.Badges)
            {
                var partialKey = string.Concat("Badge.", item.Name);
                item.DisplayName = LocalizationService.GetResourceString(string.Concat(partialKey, ".Name"));
                item.Description = LocalizationService.GetResourceString(string.Concat(partialKey, ".Desc"));
            }

            return View(new ViewMemberViewModel
            {
                User = member,
                LoggedOnUserId = loggedonId,
                Permissions = permissions
            });
        }

        /// <summary>
        ///     Add a new user
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult Register()
        {
            if (SettingsService.GetSettings().SuspendRegistration != true)
            {
                var user = MembershipService.CreateEmptyUser();

                // Populate empty viewmodel
                var viewModel = new MemberAddViewModel
                {
                    UserName = user.UserName,
                    Email = user.Email,
                    Password = user.Password,
                    IsApproved = user.IsApproved,
                    Comment = user.Comment,
                    AllRoles = RoleService.AllRoles()
                };

                // See if a return url is present or not and add it
                var returnUrl = Request["ReturnUrl"];
                if (!string.IsNullOrWhiteSpace(returnUrl))
                {
                    viewModel.ReturnUrl = returnUrl;
                }

                return View(viewModel);
            }
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        ///     Add a new user
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> Register(MemberAddViewModel userModel)
        {
            var settings = SettingsService.GetSettings();
            if (settings.SuspendRegistration != true &&
                settings.DisableStandardRegistration != true)
            {
                // First see if there is a spam question and if so, the answer matches
                if (!string.IsNullOrWhiteSpace(settings.SpamQuestion))
                {
                    // There is a spam question, if answer is wrong return with error
                    if (userModel.SpamAnswer == null ||
                        userModel.SpamAnswer.Trim() != settings.SpamAnswer)
                    {
                        // POTENTIAL SPAMMER!
                        ModelState.AddModelError(string.Empty,
                            LocalizationService.GetResourceString("Error.WrongAnswerRegistration"));
                        return View();
                    }
                }

                // Get the user model
                var user = userModel.ToMembershipUser();

                var pipeline = await MembershipService.CreateUser(user, LoginType.Standard);
                if (!pipeline.Successful)
                {
                    ModelState.AddModelError(string.Empty, pipeline.ProcessLog.FirstOrDefault());
                    return View();
                }

                // Do the register logic
                return MemberRegisterLogic(pipeline);
            }
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        ///     Social login validator which passes view model as temp data
        /// </summary>
        /// <returns></returns>
        public virtual async Task<ActionResult> SocialLoginValidator()
        {
            // Store the viewModel in TempData - Which we'll use in the register logic
            if (TempData[Constants.MemberRegisterViewModel] != null)
            {
                var userModel = TempData[Constants.MemberRegisterViewModel] as MemberAddViewModel;

                // Get the user model
                var user = userModel.ToMembershipUser();

                var pipeline = await MembershipService.CreateUser(user, userModel.LoginType);
                if (!pipeline.Successful)
                {
                    ModelState.AddModelError(string.Empty, pipeline.ProcessLog.FirstOrDefault());
                    return View("Register");
                }

                // Do the register logic
                return MemberRegisterLogic(pipeline);
            }

            ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
            return View("Register");
        }

        /// <summary>
        ///     All the logic to regsiter a member
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult MemberRegisterLogic(IPipelineProcess<MembershipUser> pipelineProcess)
        {
            // We get these from the pipelineprocess and not from the settings as they can be changed during the process (i.e. Social login)
            var manuallyAuthoriseMembers =
                pipelineProcess.ExtendedData[Constants.ExtendedDataKeys.ManuallyAuthoriseMembers] as bool?;
            var memberEmailAuthorisationNeeded =
                pipelineProcess.ExtendedData[Constants.ExtendedDataKeys.MemberEmailAuthorisationNeeded] as bool?;

            // Set the view bag message here
            SetRegisterViewBagMessage(manuallyAuthoriseMembers == true, memberEmailAuthorisationNeeded == true,
                pipelineProcess.EntityToProcess);

            // Should we redirect to the home page
            var homeRedirect = manuallyAuthoriseMembers != true && memberEmailAuthorisationNeeded != true;

            // Get the return url
            var returnUrl = pipelineProcess.EntityToProcess.GetExtendedDataItem(Constants.ExtendedDataKeys.ReturnUrl);

            try
            {
                // Remove the ReturnUrl from the user
                pipelineProcess.EntityToProcess.RemoveExtendedDataItem(Constants.ExtendedDataKeys.ReturnUrl);

                // Save any outstanding changes
                Context.SaveChanges();

                if (homeRedirect)
                {
                    if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 &&
                        returnUrl.StartsWith("/") && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home", new { area = string.Empty });
                }
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
                FormsAuthentication.SignOut();
                ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
            }

            return View("Register");
        }

        /// <summary>
        ///     Sets a view bag message based on registration result
        /// </summary>
        /// <param name="manuallyAuthoriseMembers"></param>
        /// <param name="memberEmailAuthorisationNeeded"></param>
        /// <param name="userToSave"></param>
        private void SetRegisterViewBagMessage(bool manuallyAuthoriseMembers, bool memberEmailAuthorisationNeeded,
            MembershipUser userToSave)
        {
            if (manuallyAuthoriseMembers)
            {
                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Members.NowRegisteredNeedApproval"),
                    MessageType = GenericMessages.success
                };
            }
            else if (memberEmailAuthorisationNeeded)
            {
                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Members.MemberEmailAuthorisationNeeded"),
                    MessageType = GenericMessages.success
                };
            }
            else
            {
                // If not manually authorise then log the user in
                if (ForumConfiguration.Instance.AutoLoginAfterRegister)
                {
                    FormsAuthentication.SetAuthCookie(userToSave.UserName, false);
                }

                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Members.NowRegistered"),
                    MessageType = GenericMessages.success
                };
            }
        }

        /// <summary>
        ///     Resends the email confirmation
        /// </summary>
        /// <param name="username"></param>
        /// <param name="manuallyAuthoriseMembers"></param>
        /// <param name="memberEmailAuthorisationNeeded"></param>
        /// <returns></returns>
        public virtual ActionResult ResendEmailConfirmation(string username, bool manuallyAuthoriseMembers,
            bool memberEmailAuthorisationNeeded)
        {
            try
            {
                // Get the user from the username
                var user = MembershipService.GetUser(username);

                // As this is a resend, they must have the extendeddata entry
                var registrationGuid =
                    user.GetExtendedDataItem(Constants.ExtendedDataKeys.RegistrationEmailConfirmationKey);

                if (user != null && !string.IsNullOrWhiteSpace(registrationGuid))
                {
                    _emailService.SendEmailConfirmationEmail(user, manuallyAuthoriseMembers,
                        memberEmailAuthorisationNeeded);

                    TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = LocalizationService.GetResourceString("Members.MemberEmailAuthorisationNeeded"),
                        MessageType = GenericMessages.success
                    };
                }
                else
                {
                    // Log this
                    LoggingService.Error(
                        "Unable to ResendEmailConfirmation as either user was null or RegistrationEmailConfirmationKey is missing");

                    // There was a problem
                    TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = LocalizationService.GetResourceString("Members.Errors.LogonGeneric"),
                        MessageType = GenericMessages.danger
                    };
                }

                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
            }

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        ///     Email confirmation page from the link in the users email
        /// </summary>
        /// <param name="id"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual ActionResult EmailConfirmation(Guid id, Guid key)
        {
            // Checkconfirmation
            var user = MembershipService.GetUser(id);
            if (user != null)
            {
                // Ok, now to check the Guid key
                var registrationGuid =
                    user.GetExtendedDataItem(Constants.ExtendedDataKeys.RegistrationEmailConfirmationKey);

                var everythingOk = !string.IsNullOrWhiteSpace(registrationGuid) && Guid.Parse(registrationGuid) == key;
                if (everythingOk)
                {
                    // Set the user to active
                    user.IsApproved = true;

                    // Remove the registration key
                    user.RemoveExtendedDataItem(Constants.ExtendedDataKeys.RegistrationEmailConfirmationKey);

                    // Remove the cookie
                    var myCookie =
                        new HttpCookie(Constants.MemberEmailConfirmationCookieName)
                        {
                            Expires = DateTime.UtcNow.AddDays(-1)
                        };
                    Response.Cookies.Add(myCookie);

                    // Login code
                    FormsAuthentication.SetAuthCookie(user.UserName, false);

                    // Show a new message
                    // We use temp data because we are doing a redirect
                    TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = LocalizationService.GetResourceString("Members.NowApproved"),
                        MessageType = GenericMessages.success
                    };
                }
                else
                {
                    // Show a new message
                    // We use temp data because we are doing a redirect
                    TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = LocalizationService.GetResourceString("Members.Errors.LogonGeneric"),
                        MessageType = GenericMessages.danger
                    };
                }
            }

            try
            {
                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
            }

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        ///     Log on
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult LogOn()
        {
            // Create the empty view model
            var viewModel = new LogOnViewModel();

            // See if a return url is present or not and add it
            var returnUrl = Request["ReturnUrl"];
            if (!string.IsNullOrWhiteSpace(returnUrl))
            {
                viewModel.ReturnUrl = returnUrl;
            }

            return View(viewModel);
        }

        /// <summary>
        ///     Log on post
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual async Task<ActionResult> LogOn(LogOnViewModel model)
        {
            if (ModelState.IsValid)
            {
                var message = new GenericMessageViewModel();
                var user = MembershipService.CreateEmptyUser();

                // Get the pipelines
                var userLoginPipes = ForumConfiguration.Instance.PipelinesUserLogin;

                // The model to process
                var piplineModel = new PipelineProcess<MembershipUser>(user);

                // Add the username and password
                piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.Username, model.UserName);
                piplineModel.ExtendedData.Add(Constants.ExtendedDataKeys.Password, model.Password);

                // Get instance of the pipeline to use
                var loginUserPipeline = new Pipeline<IPipelineProcess<MembershipUser>, MembershipUser>(Context);

                // Register the pipes 
                var allMembershipUserPipes =
                    ImplementationManager.GetInstances<IPipe<IPipelineProcess<MembershipUser>>>();

                // Loop through the pipes and add the ones we want
                foreach (var pipe in userLoginPipes)
                {
                    if (allMembershipUserPipes.ContainsKey(pipe))
                    {
                        loginUserPipeline.Register(allMembershipUserPipes[pipe]);
                    }
                }

                // Process the pipeline
                var loginResult = await loginUserPipeline.Process(piplineModel);

                // See if it was successful
                if (loginResult.Successful)
                {
                    // Save outstanding Changes
                    Context.SaveChanges();

                    // Login the user
                    FormsAuthentication.SetAuthCookie(model.UserName, model.RememberMe);

                    // Set the message
                    message.Message = LocalizationService.GetResourceString("Members.NowLoggedIn");
                    message.MessageType = GenericMessages.success;

                    // See if we have a return url
                    if (Url.IsLocalUrl(model.ReturnUrl) && model.ReturnUrl.Length > 1 &&
                        model.ReturnUrl.StartsWith("/")
                        && !model.ReturnUrl.StartsWith("//") && !model.ReturnUrl.StartsWith("/\\"))
                    {
                        return Redirect(model.ReturnUrl);
                    }

                    // If not just go to home page
                    return RedirectToAction("Index", "Home", new { area = string.Empty });
                }

                // Add the error if we get here
                ModelState.AddModelError(string.Empty, loginResult.ProcessLog.FirstOrDefault());
            }

            return View(model);
        }

        /// <summary>
        ///     Get: log off user
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = LocalizationService.GetResourceString("Members.NowLoggedOut"),
                MessageType = GenericMessages.success
            };
            return RedirectToAction("Index", "Home", new { area = string.Empty });
        }

        [HttpPost]
        public virtual PartialViewResult GetMemberDiscussions(Guid id)
        {
            if (Request.IsAjaxRequest())
            {
                var loggedOnReadOnlyUser = User.Identity.IsAuthenticated
                    ? MembershipService.GetUser(User.Identity.Name, true)
                    : null;
                var usersRole = loggedOnReadOnlyUser == null
                    ? RoleService.GetRole(Constants.GuestRoleName, true)
                    : loggedOnReadOnlyUser.Roles.FirstOrDefault();

                var allowedCategories = _categoryService.GetAllowedCategories(usersRole).ToList();

                // Get the user discussions, only grab 100 posts
                var posts = _postService.GetByMember(id, 100, allowedCategories);

                // Get the distinct topics
                var topics = posts.Select(x => x.Topic).Distinct().Take(6)
                    .OrderByDescending(x => x.LastPost.DateCreated).ToList();

                // Get the Topic View Models
                var topicViewModels = ViewModelMapping.CreateTopicViewModels(topics, RoleService, usersRole,
                    loggedOnReadOnlyUser, allowedCategories, SettingsService.GetSettings(), _postService,
                    _notificationService, _pollService, _voteService, _favouriteService);

                // create the view model
                var viewModel = new ViewMemberDiscussionsViewModel
                {
                    Topics = topicViewModels
                };


                return PartialView(viewModel);
            }
            return null;
        }

        /// <summary>
        ///     Edit user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public virtual ActionResult Edit(Guid id)
        {
            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);
            var loggedOnUserId = loggedOnReadOnlyUser?.Id ?? Guid.Empty;

            var permissions = RoleService.GetPermissions(null, loggedOnUsersRole);

            // Check is has permissions
            if (User.IsInRole(Constants.AdminRoleName) || loggedOnUserId == id ||
                permissions[ForumConfiguration.Instance.PermissionEditMembers].IsTicked)
            {
                var user = MembershipService.GetUser(id);
                var viewModel = user.PopulateMemberViewModel();

                return View(viewModel);
            }

            return ErrorToHomePage(LocalizationService.GetResourceString("Errors.NoPermission"));
        }

        /// <summary>
        ///     Edit user
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public virtual async Task<ActionResult> Edit(MemberFrontEndEditViewModel userModel)
        {
            // Get the user to edit from the database            
            var dbUser = MembershipService.GetUser(userModel.Id);

            // Map across the viewmodel to the user
            var user = userModel.ToMembershipUser(dbUser);

            // Repopulate any viewmodel data
            userModel.AmountOfPoints = user.TotalPoints;

            // Avatar holding image
            HttpPostedFileBase avatar = null;

            // Check image for upload
            if (userModel.Files.Any(x => x != null))
            {
                // See if file is ok and then convert to image
                avatar = userModel.Files[0];
            }

            // Edit the user via the pipelines
            var pipeline = await MembershipService.EditUser(user, User, avatar);
            if (!pipeline.Successful)
            {
                ModelState.AddModelError(string.Empty, pipeline.ProcessLog.FirstOrDefault());
                return View(userModel);
            }

            if (pipeline.ExtendedData.ContainsKey(Constants.ExtendedDataKeys.UsernameChanged))
            {
                var test = pipeline.ExtendedData[Constants.ExtendedDataKeys.UsernameChanged] as bool?;
                var usernameChanged = test == true;
                if (usernameChanged)
                {
                    // User has changed their username so need to log them in
                    // as there new username of 
                    var authCookie = Request.Cookies[FormsAuthentication.FormsCookieName];
                    if (authCookie != null)
                    {
                        var authTicket = FormsAuthentication.Decrypt(authCookie.Value);
                        if (authTicket != null)
                        {
                            var newFormsIdentity = new FormsIdentity(new FormsAuthenticationTicket(
                                authTicket.Version,
                                user.UserName,
                                authTicket.IssueDate,
                                authTicket.Expiration,
                                authTicket.IsPersistent,
                                authTicket.UserData));
                            var roles = authTicket.UserData.Split("|".ToCharArray());
                            var newGenericPrincipal = new GenericPrincipal(newFormsIdentity, roles);
                            System.Web.HttpContext.Current.User = newGenericPrincipal;
                        }
                    }

                    // sign out current user
                    FormsAuthentication.SignOut();

                    // Abandon the session
                    Session.Abandon();

                    // Sign in new user
                    FormsAuthentication.SetAuthCookie(user.UserName, false);
                }
            }

            // Repopulate any viewmodel data
            userModel.AmountOfPoints = user.TotalPoints;

            // Set the users Avatar for the confirmation page
            userModel.Avatar = user.Avatar;

            ShowMessage(new GenericMessageViewModel
            {
                Message = LocalizationService.GetResourceString("Member.ProfileUpdated"),
                MessageType = GenericMessages.success
            });

            return View(userModel);
        }

        /// <summary>
        ///     The side admin panel
        /// </summary>
        /// <param name="isDropDown"></param>
        /// <returns></returns>
        [Authorize]
        public virtual PartialViewResult SideAdminPanel(bool isDropDown)
        {
            var privateMessageCount = 0;
            var moderateCount = 0;
            var settings = SettingsService.GetSettings();
            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);
            if (loggedOnReadOnlyUser != null)
            {
                var allowedCategories = _categoryService.GetAllowedCategories(loggedOnUsersRole);
                privateMessageCount = _privateMessageService.NewPrivateMessageCount(loggedOnReadOnlyUser.Id);
                var pendingTopics = _topicService.GetPendingTopics(allowedCategories, loggedOnUsersRole);
                var pendingPosts = _postService.GetPendingPosts(allowedCategories, loggedOnUsersRole);
                moderateCount = pendingTopics.Count + pendingPosts.Count;
            }

            var canViewPms = settings.EnablePrivateMessages && loggedOnReadOnlyUser != null &&
                             loggedOnReadOnlyUser.DisablePrivateMessages != true;
            var viewModel = new ViewAdminSidePanelViewModel
            {
                CurrentUser = loggedOnReadOnlyUser,
                NewPrivateMessageCount = canViewPms ? privateMessageCount : 0,
                CanViewPrivateMessages = canViewPms,
                ModerateCount = moderateCount,
                IsDropDown = isDropDown
            };

            return PartialView(viewModel);
        }

        /// <summary>
        ///     Member profile tools
        /// </summary>
        /// <returns></returns>
        public virtual PartialViewResult AdminMemberProfileTools()
        {
            return PartialView();
        }

        /// <summary>
        ///     Autocomplete
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        [Authorize]
        public virtual string AutoComplete(string term)
        {
            if (!string.IsNullOrWhiteSpace(term))
            {
                var members = MembershipService.SearchMembers(term, 12);
                var sb = new StringBuilder();
                sb.Append("[").Append(Environment.NewLine);
                for (var i = 0; i < members.Count; i++)
                {
                    sb.AppendFormat("\"{0}\"", members[i].UserName);
                    if (i < members.Count - 1)
                    {
                        sb.Append(",");
                    }
                    sb.Append(Environment.NewLine);
                }
                sb.Append("]");
                return sb.ToString();
            }
            return null;
        }

        /// <summary>
        ///     Report a member
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public virtual ActionResult Report(Guid id)
        {
            if (SettingsService.GetSettings().EnableMemberReporting)
            {
                var user = MembershipService.GetUser(id);
                return View(new ReportMemberViewModel { Id = user.Id, Username = user.UserName });
            }
            return ErrorToHomePage(LocalizationService.GetResourceString("Errors.GenericMessage"));
        }

        /// <summary>
        ///     Report a member
        /// </summary>
        /// <param name="viewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public virtual ActionResult Report(ReportMemberViewModel viewModel)
        {
            if (SettingsService.GetSettings().EnableMemberReporting)
            {
                var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);

                var user = MembershipService.GetUser(viewModel.Id);
                var report = new Report
                {
                    Reason = viewModel.Reason,
                    ReportedMember = user,
                    Reporter = loggedOnReadOnlyUser
                };
                _reportService.MemberReport(report);

                try
                {
                    Context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Context.RollBack();
                    LoggingService.Error(ex);
                }

                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Report.ReportSent"),
                    MessageType = GenericMessages.success
                };
                return View(new ReportMemberViewModel { Id = user.Id, Username = user.UserName });
            }
            return ErrorToHomePage(LocalizationService.GetResourceString("Errors.GenericMessage"));
        }

        /// <summary>
        ///     Member search
        /// </summary>
        /// <param name="p"></param>
        /// <param name="search"></param>
        /// <returns></returns>
        [Authorize]
        public virtual async Task<ActionResult> Search(int? p, string search)
        {
            var pageIndex = p ?? 1;
            var allUsers = string.IsNullOrWhiteSpace(search)
                ? await MembershipService.GetAll(pageIndex, ForumConfiguration.Instance.AdminListPageSize)
                : await MembershipService.SearchMembers(search, pageIndex,
                    ForumConfiguration.Instance.AdminListPageSize);

            // Redisplay list of users
            var allViewModelUsers = allUsers.Select(user => new PublicSingleMemberListViewModel
            {
                UserName = user.UserName,
                NiceUrl = user.NiceUrl,
                CreateDate = user.CreateDate,
                TotalPoints = user.TotalPoints
            }).ToList();

            var memberListModel = new PublicMemberListViewModel
            {
                Users = allViewModelUsers,
                PageIndex = pageIndex,
                TotalCount = allUsers.TotalCount,
                Search = search
            };

            return View(memberListModel);
        }

        /// <summary>
        ///     Latest members joined
        /// </summary>
        /// <returns></returns>
        [ChildActionOnly]
        public virtual PartialViewResult LatestMembersJoined()
        {
            var viewModel = new ListLatestMembersViewModel();
            var users = MembershipService.GetLatestUsers(10).ToDictionary(o => o.UserName, o => o.NiceUrl);
            viewModel.Users = users;
            return PartialView(viewModel);
        }

        /// <summary>
        ///     Change password view
        /// </summary>
        /// <returns></returns>
        [Authorize]
        public virtual ActionResult ChangePassword()
        {
            return View();
        }

        /// <summary>
        ///     Change password logic
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public virtual ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var changePasswordSucceeded = true;

            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            if (ModelState.IsValid)
            {
                // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                var loggedOnUser = MembershipService.GetUser(loggedOnReadOnlyUser.Id);
                changePasswordSucceeded =
                    MembershipService.ChangePassword(loggedOnUser, model.OldPassword, model.NewPassword);

                try
                {
                    Context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Context.RollBack();
                    LoggingService.Error(ex);
                    changePasswordSucceeded = false;
                }
            }

            if (changePasswordSucceeded)
            {
                // We use temp data because we are doing a redirect
                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Members.ChangePassword.Success"),
                    MessageType = GenericMessages.success
                };
                return View();
            }

            ModelState.AddModelError("", LocalizationService.GetResourceString("Members.ChangePassword.Error"));
            return View(model);
        }

        /// <summary>
        ///     Forgot password view
        /// </summary>
        /// <returns></returns>
        public virtual ActionResult ForgotPassword()
        {
            return View();
        }

        /// <summary>
        ///     Forgot password logic
        /// </summary>
        /// <param name="forgotPasswordViewModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult ForgotPassword(ForgotPasswordViewModel forgotPasswordViewModel)
        {
            if (!ModelState.IsValid)
            {
                return View(forgotPasswordViewModel);
            }

            var user = MembershipService.GetUserByEmail(forgotPasswordViewModel.EmailAddress);

            // If the email address is not registered then display the 'email sent' confirmation the same as if 
            // the email address was registered. There is no harm in doing this and it avoids exposing registered 
            // email addresses which could be a privacy issue if the forum is of a sensitive nature. */
            if (user == null)
            {
                return RedirectToAction("PasswordResetSent", "Members");
            }

            try
            {
                // If the user is registered then create a security token and a timestamp that will allow a change of password
                MembershipService.UpdatePasswordResetToken(user);
                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
                ModelState.AddModelError("", LocalizationService.GetResourceString("Members.ResetPassword.Error"));
                return View(forgotPasswordViewModel);
            }

            var settings = SettingsService.GetSettings();
            var url = new Uri(string.Concat(settings.ForumUrl.TrimEnd('/'),
                Url.Action("ResetPassword", "Members", new { user.Id, token = user.PasswordResetToken })));

            var sb = new StringBuilder();
            sb.AppendFormat("<p>{0}</p>",
                string.Format(LocalizationService.GetResourceString("Members.ResetPassword.EmailText"),
                    settings.ForumName));
            sb.AppendFormat("<p><a href=\"{0}\">{0}</a></p>", url);

            var email = new Email
            {
                EmailTo = user.Email,
                NameTo = user.UserName,
                Subject = LocalizationService.GetResourceString("Members.ForgotPassword.Subject")
            };
            email.Body = _emailService.EmailTemplate(email.NameTo, sb.ToString());
            _emailService.SendMail(email);

            try
            {
                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
                ModelState.AddModelError("", LocalizationService.GetResourceString("Members.ResetPassword.Error"));
                return View(forgotPasswordViewModel);
            }


            return RedirectToAction("PasswordResetSent", "Members");
        }

        /// <summary>
        ///     Password resent
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public virtual ViewResult PasswordResetSent()
        {
            return View();
        }

        /// <summary>
        ///     Password reset view
        /// </summary>
        /// <param name="id"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        [HttpGet]
        public virtual ViewResult ResetPassword(Guid? id, string token)
        {
            var model = new ResetPasswordViewModel
            {
                Id = id,
                Token = token
            };

            if (id == null || string.IsNullOrWhiteSpace(token))
            {
                ModelState.AddModelError("",
                    LocalizationService.GetResourceString("Members.ResetPassword.InvalidToken"));
            }

            return View(model);
        }

        /// <summary>
        ///     Password reset logic
        /// </summary>
        /// <param name="postedModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public virtual ActionResult ResetPassword(ResetPasswordViewModel postedModel)
        {
            if (!ModelState.IsValid)
            {
                return View(postedModel);
            }


            if (postedModel.Id != null)
            {
                var user = MembershipService.GetUser(postedModel.Id.Value);

                // if the user id wasn't found then we can't proceed
                // if the token submitted is not valid then do not proceed
                if (user?.PasswordResetToken == null ||
                    !MembershipService.IsPasswordResetTokenValid(user, postedModel.Token))
                {
                    ModelState.AddModelError("",
                        LocalizationService.GetResourceString("Members.ResetPassword.InvalidToken"));
                    return View(postedModel);
                }

                try
                {
                    // The security token is valid so change the password
                    MembershipService.ResetPassword(user, postedModel.NewPassword);
                    // Clear the token and the timestamp so that the URL cannot be used again
                    MembershipService.ClearPasswordResetToken(user);
                    Context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Context.RollBack();
                    LoggingService.Error(ex);
                    ModelState.AddModelError("",
                        LocalizationService.GetResourceString("Members.ResetPassword.InvalidToken"));
                    return View(postedModel);
                }
            }


            return RedirectToAction("PasswordChanged", "Members");
        }

        /// <summary>
        ///     Password changed view
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public virtual ViewResult PasswordChanged()
        {
            return View();
        }
    }
}