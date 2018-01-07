namespace MvcForum.Web.Controllers
{
    using System;
    using System.Drawing.Imaging;
    using System.IO;
    using System.Linq;
    using System.Security.Principal;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Hosting;
    using System.Web.Mvc;
    using System.Web.Security;
    using Application;
    using Areas.Admin.ViewModels;
    using Core.Constants;
    using Core.Events;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Core.Models;
    using Core.Models.Entities;
    using Core.Models.Enums;
    using Core.Models.General;
    using Core.Utilities;
    using ViewModels.Admin;
    using ViewModels.Mapping;
    using ViewModels.Member;
    using ViewModels.Registration;
    using MembershipCreateStatus = Core.Models.Enums.MembershipCreateStatus;
    using MembershipUser = Core.Models.Entities.MembershipUser;

    /// <summary>
    ///     Members controller
    /// </summary>
    public partial class MembersController : BaseController
    {
        private readonly IBannedEmailService _bannedEmailService;
        private readonly IBannedWordService _bannedWordService;
        private readonly ICategoryService _categoryService;
        private readonly IEmailService _emailService;
        private readonly IFavouriteService _favouriteService;
        private readonly IPollAnswerService _pollAnswerService;
        private readonly IPostService _postService;
        private readonly IPrivateMessageService _privateMessageService;
        private readonly IReportService _reportService;
        private readonly ITopicNotificationService _topicNotificationService;
        private readonly ITopicService _topicService;
        private readonly IVoteService _voteService;

        public MembersController(ILoggingService loggingService, IMembershipService membershipService,
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService,
            IPostService postService, IReportService reportService, IEmailService emailService,
            IPrivateMessageService privateMessageService, IBannedEmailService bannedEmailService,
            IBannedWordService bannedWordService, ICategoryService categoryService, ITopicService topicService,
            ICacheService cacheService, ITopicNotificationService topicNotificationService,
            IPollAnswerService pollAnswerService, IVoteService voteService, IFavouriteService favouriteService,
            IMvcForumContext context)
            : base(loggingService, membershipService, localizationService, roleService,
                settingsService, cacheService, context)
        {
            _postService = postService;
            _reportService = reportService;
            _emailService = emailService;
            _privateMessageService = privateMessageService;
            _bannedEmailService = bannedEmailService;
            _bannedWordService = bannedWordService;
            _categoryService = categoryService;
            _topicService = topicService;
            _topicNotificationService = topicNotificationService;
            _pollAnswerService = pollAnswerService;
            _voteService = voteService;
            _favouriteService = favouriteService;
        }

        /// <summary>
        ///     Scrubs a user and bans them
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize(Roles = AppConstants.AdminRoleName)]
        public ActionResult SrubAndBanUser(Guid id)
        {
            var user = MembershipService.GetUser(id);


            if (!user.Roles.Any(x => x.RoleName.Contains(AppConstants.AdminRoleName)))
            {
                MembershipService.ScrubUsers(user);

                try
                {
                    Context.SaveChanges();
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = LocalizationService.GetResourceString("Members.SuccessfulSrub"),
                        MessageType = GenericMessages.success
                    };
                }
                catch (Exception ex)
                {
                    Context.RollBack();
                    LoggingService.Error(ex);
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = LocalizationService.GetResourceString("Members.UnSuccessfulSrub"),
                        MessageType = GenericMessages.danger
                    };
                }
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
        public ActionResult BanMember(Guid id)
        {
            var user = MembershipService.GetUser(id);
            var currentUser = MembershipService.GetUser(User.Identity.Name, true);
            var permissions = RoleService.GetPermissions(null, currentUser.Roles.FirstOrDefault());

            if (permissions[SiteConstants.Instance.PermissionEditMembers].IsTicked)
            {
                if (!user.Roles.Any(x => x.RoleName.Contains(AppConstants.AdminRoleName)))
                {
                    user.IsBanned = true;

                    try
                    {
                        Context.SaveChanges();
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = LocalizationService.GetResourceString("Members.NowBanned"),
                            MessageType = GenericMessages.success
                        };
                    }
                    catch (Exception ex)
                    {
                        Context.RollBack();
                        LoggingService.Error(ex);
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
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
        public ActionResult UnBanMember(Guid id)
        {
            var user = MembershipService.GetUser(id);
            var currentUser = MembershipService.GetUser(User.Identity.Name, true);
            var permissions = RoleService.GetPermissions(null, currentUser.Roles.FirstOrDefault());

            if (permissions[SiteConstants.Instance.PermissionEditMembers].IsTicked)
            {
                if (!user.Roles.Any(x => x.RoleName.Contains(AppConstants.AdminRoleName)))
                {
                    user.IsBanned = false;

                    try
                    {
                        Context.SaveChanges();
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = LocalizationService.GetResourceString("Members.NowUnBanned"),
                            MessageType = GenericMessages.success
                        };
                    }
                    catch (Exception ex)
                    {
                        Context.RollBack();
                        LoggingService.Error(ex);
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
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
        public PartialViewResult GetCurrentActiveMembers()
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
        public JsonResult LastActiveCheck()
        {
            if (User.Identity.IsAuthenticated)
            {
                var rightNow = DateTime.UtcNow;
                var currentUser = MembershipService.GetUser(User.Identity.Name, true);
                var usersDate = currentUser.LastActivityDate ?? DateTime.UtcNow.AddDays(-1);

                var span = rightNow.Subtract(usersDate);
                var totalMins = span.TotalMinutes;

                if (totalMins > AppConstants.TimeSpanInMinutesToDoCheck)
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
            return Json(new {Timer = "reset"}, JsonRequestBehavior.AllowGet);
        }

        /// <summary>
        ///     Gets a member by slug
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public ActionResult GetByName(string slug)
        {
            var member = MembershipService.GetUserBySlug(slug);
            var loggedOnReadOnlyUser = User.Identity.IsAuthenticated
                ? MembershipService.GetUser(User.Identity.Name, true)
                : null;
            var usersRole = loggedOnReadOnlyUser == null
                ? RoleService.GetRole(AppConstants.GuestRoleName, true)
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
        public ActionResult Register()
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
        public ActionResult Register(MemberAddViewModel userModel)
        {
            if (SettingsService.GetSettings().SuspendRegistration != true &&
                SettingsService.GetSettings().DisableStandardRegistration != true)
            {
                // First see if there is a spam question and if so, the answer matches
                if (!string.IsNullOrWhiteSpace(SettingsService.GetSettings().SpamQuestion))
                {
                    // There is a spam question, if answer is wrong return with error
                    if (userModel.SpamAnswer == null ||
                        userModel.SpamAnswer.Trim() != SettingsService.GetSettings().SpamAnswer)
                    {
                        // POTENTIAL SPAMMER!
                        ModelState.AddModelError(string.Empty,
                            LocalizationService.GetResourceString("Error.WrongAnswerRegistration"));
                        return View();
                    }
                }

                // Secondly see if the email is banned
                if (_bannedEmailService.EmailIsBanned(userModel.Email))
                {
                    ModelState.AddModelError(string.Empty,
                        LocalizationService.GetResourceString("Error.EmailIsBanned"));
                    return View();
                }


                // Standard Login
                userModel.LoginType = LoginType.Standard;

                // Do the register logic
                return MemberRegisterLogic(userModel);
            }
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        ///     Social login validator which passes view model as temp data
        /// </summary>
        /// <returns></returns>
        public ActionResult SocialLoginValidator()
        {
            // Store the viewModel in TempData - Which we'll use in the register logic
            if (TempData[AppConstants.MemberRegisterViewModel] != null)
            {
                var userModel = TempData[AppConstants.MemberRegisterViewModel] as MemberAddViewModel;

                // Do the register logic
                return MemberRegisterLogic(userModel);
            }


            ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
            return View("Register");
        }

        /// <summary>
        ///     All the logic to regsiter a member
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        public ActionResult MemberRegisterLogic(MemberAddViewModel userModel)
        {
            var settings = SettingsService.GetSettings();
            var manuallyAuthoriseMembers = settings.ManuallyAuthoriseNewMembers;
            var memberEmailAuthorisationNeeded = settings.NewMemberEmailConfirmation == true;
            var homeRedirect = false;

            var userToSave = new MembershipUser
            {
                UserName = _bannedWordService.SanitiseBannedWords(userModel.UserName),
                Email = userModel.Email,
                Password = userModel.Password,
                IsApproved = userModel.IsApproved,
                Comment = userModel.Comment
            };

            var createStatus = MembershipService.CreateUser(userToSave);
            if (createStatus != MembershipCreateStatus.Success)
            {
                ModelState.AddModelError(string.Empty, MembershipService.ErrorCodeToString(createStatus));
            }
            else
            {
                // See if this is a social login and we have their profile pic
                if (!string.IsNullOrWhiteSpace(userModel.SocialProfileImageUrl))
                {
                    // We have an image url - Need to save it to their profile
                    var image = AppHelpers.GetImageFromExternalUrl(userModel.SocialProfileImageUrl);

                    // Set upload directory - Create if it doesn't exist
                    var uploadFolderPath =
                        HostingEnvironment.MapPath(string.Concat(SiteConstants.Instance.UploadFolderPath,
                            userToSave.Id));
                    if (uploadFolderPath != null && !Directory.Exists(uploadFolderPath))
                    {
                        Directory.CreateDirectory(uploadFolderPath);
                    }

                    // Get the file name
                    var fileName = Path.GetFileName(userModel.SocialProfileImageUrl);

                    // Create a HttpPostedFileBase image from the C# Image
                    using (var stream = new MemoryStream())
                    {
                        // Microsoft doesn't give you a file extension - See if it has a file extension
                        // Get the file extension
                        var fileExtension = Path.GetExtension(fileName);

                        // Fix invalid Illegal charactors
                        var regexSearch = new string(Path.GetInvalidFileNameChars()) +
                                          new string(Path.GetInvalidPathChars());
                        var reg = new Regex($"[{Regex.Escape(regexSearch)}]");
                        fileName = reg.Replace(fileName, "");

                        if (string.IsNullOrWhiteSpace(fileExtension))
                        {
                            // no file extension so give it one
                            fileName = string.Concat(fileName, ".jpg");
                        }

                        image.Save(stream, ImageFormat.Jpeg);
                        stream.Position = 0;
                        HttpPostedFileBase formattedImage = new MemoryFile(stream, "image/jpeg", fileName);

                        // Upload the file
                        var uploadResult = AppHelpers.UploadFile(formattedImage, uploadFolderPath,
                            LocalizationService, true);

                        // Don't throw error if problem saving avatar, just don't save it.
                        if (uploadResult.UploadSuccessful)
                        {
                            userToSave.Avatar = uploadResult.UploadedFileName;
                        }
                    }
                }

                // Store access token for social media account in case we want to do anything with it
                var isSocialLogin = false;
                if (userModel.LoginType == LoginType.Facebook)
                {
                    userToSave.FacebookAccessToken = userModel.UserAccessToken;
                    isSocialLogin = true;
                }
                if (userModel.LoginType == LoginType.Google)
                {
                    userToSave.GoogleAccessToken = userModel.UserAccessToken;
                    isSocialLogin = true;
                }
                if (userModel.LoginType == LoginType.Microsoft)
                {
                    userToSave.MicrosoftAccessToken = userModel.UserAccessToken;
                    isSocialLogin = true;
                }

                // If this is a social login, and memberEmailAuthorisationNeeded is true then we need to ignore it
                // and set memberEmailAuthorisationNeeded to false because the email addresses are validated by the social media providers
                if (isSocialLogin && !manuallyAuthoriseMembers)
                {
                    memberEmailAuthorisationNeeded = false;
                    userToSave.IsApproved = true;
                }

                // Set the view bag message here
                SetRegisterViewBagMessage(manuallyAuthoriseMembers, memberEmailAuthorisationNeeded, userToSave);

                if (!manuallyAuthoriseMembers && !memberEmailAuthorisationNeeded)
                {
                    homeRedirect = true;
                }

                try
                {
                    // Only send the email if the admin is not manually authorising emails or it's pointless
                    SendEmailConfirmationEmail(userToSave);

                    Context.SaveChanges();

                    if (homeRedirect)
                    {
                        if (Url.IsLocalUrl(userModel.ReturnUrl) && userModel.ReturnUrl.Length > 1 &&
                            userModel.ReturnUrl.StartsWith("/")
                            && !userModel.ReturnUrl.StartsWith("//") && !userModel.ReturnUrl.StartsWith("/\\"))
                        {
                            return Redirect(userModel.ReturnUrl);
                        }
                        return RedirectToAction("Index", "Home", new {area = string.Empty});
                    }
                }
                catch (Exception ex)
                {
                    Context.RollBack();
                    LoggingService.Error(ex);
                    FormsAuthentication.SignOut();
                    ModelState.AddModelError(string.Empty,
                        LocalizationService.GetResourceString("Errors.GenericMessage"));
                }
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
                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Members.NowRegisteredNeedApproval"),
                    MessageType = GenericMessages.success
                };
            }
            else if (memberEmailAuthorisationNeeded)
            {
                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Members.MemberEmailAuthorisationNeeded"),
                    MessageType = GenericMessages.success
                };
            }
            else
            {
                // If not manually authorise then log the user in
                if (SiteConstants.Instance.AutoLoginAfterRegister)
                {
                    FormsAuthentication.SetAuthCookie(userToSave.UserName, false);
                }

                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Members.NowRegistered"),
                    MessageType = GenericMessages.success
                };
            }
        }

        /// <summary>
        ///     Sends registration confirmation email
        /// </summary>
        /// <param name="userToSave"></param>
        private void SendEmailConfirmationEmail(MembershipUser userToSave)
        {
            var settings = SettingsService.GetSettings();
            var manuallyAuthoriseMembers = settings.ManuallyAuthoriseNewMembers;
            var memberEmailAuthorisationNeeded = settings.NewMemberEmailConfirmation == true;
            if (manuallyAuthoriseMembers == false && memberEmailAuthorisationNeeded)
            {
                if (!string.IsNullOrWhiteSpace(userToSave.Email))
                {
                    // Registration guid
                    var registrationGuid = Guid.NewGuid().ToString();

                    // Set a Guid in the extended data
                    userToSave.SetExtendedDataValue(AppConstants.ExtendedDataKeys.RegistrationEmailConfirmationKey,
                        registrationGuid);

                    // SEND AUTHORISATION EMAIL
                    var sb = new StringBuilder();
                    var confirmationLink = string.Concat(StringUtils.ReturnCurrentDomain(),
                        Url.Action("EmailConfirmation", new {id = userToSave.Id, key = registrationGuid}));

                    sb.AppendFormat("<p>{0}</p>", string.Format(
                        LocalizationService.GetResourceString("Members.MemberEmailAuthorisation.EmailBody"),
                        settings.ForumName,
                        string.Format("<p><a href=\"{0}\">{0}</a></p>", confirmationLink)));

                    var email = new Email
                    {
                        EmailTo = userToSave.Email,
                        NameTo = userToSave.UserName,
                        Subject = LocalizationService.GetResourceString("Members.MemberEmailAuthorisation.Subject")
                    };

                    email.Body = _emailService.EmailTemplate(email.NameTo, sb.ToString());

                    _emailService.SendMail(email);

                    // We add a cookie for 7 days, which will display the resend email confirmation button
                    // This cookie is removed when they click the confirmation link
                    var myCookie = new HttpCookie(AppConstants.MemberEmailConfirmationCookieName)
                    {
                        Value = userToSave.UserName,
                        Expires = DateTime.UtcNow.AddDays(7)
                    };

                    // Add the cookie.
                    Response.Cookies.Add(myCookie);

                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = LocalizationService.GetResourceString("Members.MemberEmailAuthorisationNeeded"),
                        MessageType = GenericMessages.success
                    };
                }
            }
        }

        /// <summary>
        ///     Resends the email confirmation
        /// </summary>
        /// <param name="username"></param>
        /// <returns></returns>
        public ActionResult ResendEmailConfirmation(string username)
        {
            try
            {
                // Get the user from the username
                var user = MembershipService.GetUser(username);

                // As this is a resend, they must have the extendeddata entry
                var registrationGuid =
                    user.GetExtendedDataItem(AppConstants.ExtendedDataKeys.RegistrationEmailConfirmationKey);

                if (user != null && !string.IsNullOrWhiteSpace(registrationGuid))
                {
                    SendEmailConfirmationEmail(user);
                }
                else
                {
                    // Log this
                    LoggingService.Error(
                        "Unable to ResendEmailConfirmation as either user was null or RegistrationEmailConfirmationKey is missing");

                    // There was a problem
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
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
        public ActionResult EmailConfirmation(Guid id, Guid key)
        {
            // Checkconfirmation
            var user = MembershipService.GetUser(id);
            if (user != null)
            {
                // Ok, now to check the Guid key
                var registrationGuid =
                    user.GetExtendedDataItem(AppConstants.ExtendedDataKeys.RegistrationEmailConfirmationKey);

                var everythingOk = !string.IsNullOrWhiteSpace(registrationGuid) && Guid.Parse(registrationGuid) == key;
                if (everythingOk)
                {
                    // Set the user to active
                    user.IsApproved = true;

                    // Remove the registration key
                    user.RemoveExtendedDataItem(AppConstants.ExtendedDataKeys.RegistrationEmailConfirmationKey);

                    // Remove the cookie
                    var myCookie =
                        new HttpCookie(AppConstants.MemberEmailConfirmationCookieName)
                        {
                            Expires = DateTime.UtcNow.AddDays(-1)
                        };
                    Response.Cookies.Add(myCookie);

                    // Login code
                    FormsAuthentication.SetAuthCookie(user.UserName, false);

                    // Show a new message
                    // We use temp data because we are doing a redirect
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = LocalizationService.GetResourceString("Members.NowApproved"),
                        MessageType = GenericMessages.success
                    };
                }
                else
                {
                    // Show a new message
                    // We use temp data because we are doing a redirect
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
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
        public ActionResult LogOn()
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
        public ActionResult LogOn(LogOnViewModel model)
        {
            var username = model.UserName;
            var password = model.Password;

            try
            {
                if (ModelState.IsValid)
                {
                    // We have an event here to help with Single Sign Ons
                    // You can do manual lookups to check users based on a webservice and validate a user
                    // Then log them in if they exist or create them and log them in - Have passed in a UnitOfWork
                    // To allow database changes.

                    var e = new LoginEventArgs
                    {
                        UserName = model.UserName,
                        Password = model.Password,
                        RememberMe = model.RememberMe,
                        ReturnUrl = model.ReturnUrl,
                        MvcForumContext = Context
                    };
                    EventManager.Instance.FireBeforeLogin(this, e);

                    if (!e.Cancel)
                    {
                        var message = new GenericMessageViewModel();
                        var user = new MembershipUser();
                        if (MembershipService.ValidateUser(username, password,
                            Membership.MaxInvalidPasswordAttempts))
                        {
                            // Set last login date
                            user = MembershipService.GetUser(username);
                            if (user.IsApproved && !user.IsLockedOut && !user.IsBanned)
                            {
                                FormsAuthentication.SetAuthCookie(username, model.RememberMe);
                                user.LastLoginDate = DateTime.UtcNow;

                                if (Url.IsLocalUrl(model.ReturnUrl) && model.ReturnUrl.Length > 1 &&
                                    model.ReturnUrl.StartsWith("/")
                                    && !model.ReturnUrl.StartsWith("//") && !model.ReturnUrl.StartsWith("/\\"))
                                {
                                    return Redirect(model.ReturnUrl);
                                }

                                message.Message = LocalizationService.GetResourceString("Members.NowLoggedIn");
                                message.MessageType = GenericMessages.success;

                                EventManager.Instance.FireAfterLogin(this, new LoginEventArgs
                                {
                                    UserName = model.UserName,
                                    Password = model.Password,
                                    RememberMe = model.RememberMe,
                                    ReturnUrl = model.ReturnUrl,
                                    MvcForumContext = Context
                                });

                                return RedirectToAction("Index", "Home", new {area = string.Empty});
                            }
                        }

                        // Only show if we have something to actually show to the user
                        if (!string.IsNullOrWhiteSpace(message.Message))
                        {
                            TempData[AppConstants.MessageViewBagName] = message;
                        }
                        else
                        {
                            // get here Login failed, check the login status
                            var loginStatus = MembershipService.LastLoginStatus;

                            switch (loginStatus)
                            {
                                case LoginAttemptStatus.UserNotFound:
                                case LoginAttemptStatus.PasswordIncorrect:
                                    ModelState.AddModelError(string.Empty,
                                        LocalizationService.GetResourceString("Members.Errors.PasswordIncorrect"));
                                    break;

                                case LoginAttemptStatus.PasswordAttemptsExceeded:
                                    ModelState.AddModelError(string.Empty,
                                        LocalizationService.GetResourceString(
                                            "Members.Errors.PasswordAttemptsExceeded"));
                                    break;

                                case LoginAttemptStatus.UserLockedOut:
                                    ModelState.AddModelError(string.Empty,
                                        LocalizationService.GetResourceString("Members.Errors.UserLockedOut"));
                                    break;

                                case LoginAttemptStatus.Banned:
                                    ModelState.AddModelError(string.Empty,
                                        LocalizationService.GetResourceString("Members.NowBanned"));
                                    break;

                                case LoginAttemptStatus.UserNotApproved:
                                    ModelState.AddModelError(string.Empty,
                                        LocalizationService.GetResourceString("Members.Errors.UserNotApproved"));
                                    user = MembershipService.GetUser(username);
                                    SendEmailConfirmationEmail(user);
                                    break;

                                default:
                                    ModelState.AddModelError(string.Empty,
                                        LocalizationService.GetResourceString("Members.Errors.LogonGeneric"));
                                    break;
                            }
                        }
                    }
                }
            }

            finally
            {
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

            return View(model);
        }

        /// <summary>
        ///     Get: log off user
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOff()
        {
            FormsAuthentication.SignOut();
            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = LocalizationService.GetResourceString("Members.NowLoggedOut"),
                MessageType = GenericMessages.success
            };
            return RedirectToAction("Index", "Home", new {area = string.Empty});
        }

        [HttpPost]
        public PartialViewResult GetMemberDiscussions(Guid id)
        {
            if (Request.IsAjaxRequest())
            {
                var loggedOnReadOnlyUser = User.Identity.IsAuthenticated
                    ? MembershipService.GetUser(User.Identity.Name, true)
                    : null;
                var usersRole = loggedOnReadOnlyUser == null
                    ? RoleService.GetRole(AppConstants.GuestRoleName, true)
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
                    _topicNotificationService, _pollAnswerService, _voteService, _favouriteService);

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
        ///     Creates view model
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        private static MemberFrontEndEditViewModel PopulateMemberViewModel(MembershipUser user)
        {
            var viewModel = new MemberFrontEndEditViewModel
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Signature = user.Signature,
                Age = user.Age,
                Location = user.Location,
                Website = user.Website,
                Twitter = user.Twitter,
                Facebook = user.Facebook,
                DisableFileUploads = user.DisableFileUploads == true,
                Avatar = user.Avatar,
                DisableEmailNotifications = user.DisableEmailNotifications == true,
                AmountOfPoints = user.TotalPoints
            };
            return viewModel;
        }

        /// <summary>
        ///     Edit user
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Authorize]
        public ActionResult Edit(Guid id)
        {
            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);
            var loggedOnUserId = loggedOnReadOnlyUser?.Id ?? Guid.Empty;

            var permissions = RoleService.GetPermissions(null, loggedOnUsersRole);

            // Check is has permissions
            if (User.IsInRole(AppConstants.AdminRoleName) || loggedOnUserId == id ||
                permissions[SiteConstants.Instance.PermissionEditMembers].IsTicked)
            {
                var user = MembershipService.GetUser(id);
                var viewModel = PopulateMemberViewModel(user);

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
        public ActionResult Edit(MemberFrontEndEditViewModel userModel)
        {
            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);
            var loggedOnUserId = loggedOnReadOnlyUser?.Id ?? Guid.Empty;
            var permissions = RoleService.GetPermissions(null, loggedOnUsersRole);

            // Check is has permissions
            if (User.IsInRole(AppConstants.AdminRoleName) || loggedOnUserId == userModel.Id ||
                permissions[SiteConstants.Instance.PermissionEditMembers].IsTicked)
            {
                // Get the user from DB
                var user = MembershipService.GetUser(userModel.Id);

                // Before we do anything - Check stop words
                var stopWords = _bannedWordService.GetAll(true);
                var bannedWords = _bannedWordService.GetAll().Select(x => x.Word).ToList();

                // Check the fields for bad words
                foreach (var stopWord in stopWords)
                {
                    if (userModel.Facebook != null &&
                        userModel.Facebook.IndexOf(stopWord.Word, StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                        userModel.Location != null && userModel.Location.IndexOf(stopWord.Word,
                            StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                        userModel.Signature != null && userModel.Signature.IndexOf(stopWord.Word,
                            StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                        userModel.Twitter != null && userModel.Twitter.IndexOf(stopWord.Word,
                            StringComparison.CurrentCultureIgnoreCase) >= 0 ||
                        userModel.Website != null && userModel.Website.IndexOf(stopWord.Word,
                            StringComparison.CurrentCultureIgnoreCase) >= 0)
                    {
                        ShowMessage(new GenericMessageViewModel
                        {
                            Message = LocalizationService.GetResourceString("StopWord.Error"),
                            MessageType = GenericMessages.danger
                        });

                        // Ahhh found a stop word. Abandon operation captain.
                        return View(userModel);
                    }
                }

                // Repopulate any viewmodel data
                userModel.AmountOfPoints = user.TotalPoints;

                // Sort image out first
                if (userModel.Files != null)
                {
                    // Before we save anything, check the user already has an upload folder and if not create one
                    var uploadFolderPath =
                        HostingEnvironment.MapPath(string.Concat(SiteConstants.Instance.UploadFolderPath,
                            loggedOnReadOnlyUser.Id));
                    if (!Directory.Exists(uploadFolderPath))
                    {
                        Directory.CreateDirectory(uploadFolderPath);
                    }

                    // Loop through each file and get the file info and save to the users folder and Db
                    var file = userModel.Files[0];
                    if (file != null)
                    {
                        // If successful then upload the file
                        var uploadResult = AppHelpers.UploadFile(file, uploadFolderPath, LocalizationService, true);

                        if (!uploadResult.UploadSuccessful)
                        {
                            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                            {
                                Message = uploadResult.ErrorMessage,
                                MessageType = GenericMessages.danger
                            };
                            return View(userModel);
                        }

                        // Save avatar to user
                        user.Avatar = uploadResult.UploadedFileName;
                    }
                }

                // Set the users Avatar for the confirmation page
                userModel.Avatar = user.Avatar;

                // Update other users properties
                user.Age = userModel.Age;
                user.Facebook = _bannedWordService.SanitiseBannedWords(userModel.Facebook, bannedWords);
                user.Location = _bannedWordService.SanitiseBannedWords(userModel.Location, bannedWords);
                user.Signature =
                    _bannedWordService.SanitiseBannedWords(StringUtils.ScrubHtml(userModel.Signature, true),
                        bannedWords);
                user.Twitter = _bannedWordService.SanitiseBannedWords(userModel.Twitter, bannedWords);
                user.Website = _bannedWordService.SanitiseBannedWords(userModel.Website, bannedWords);
                user.DisableEmailNotifications = userModel.DisableEmailNotifications;

                // User is trying to change username, need to check if a user already exists
                // with the username they are trying to change to
                var changedUsername = false;
                var sanitisedUsername = _bannedWordService.SanitiseBannedWords(userModel.UserName, bannedWords);
                if (sanitisedUsername != user.UserName)
                {
                    if (MembershipService.GetUser(sanitisedUsername) != null)
                    {
                        Context.RollBack();
                        ModelState.AddModelError(string.Empty,
                            LocalizationService.GetResourceString("Members.Errors.DuplicateUserName"));
                        return View(userModel);
                    }

                    user.UserName = sanitisedUsername;
                    changedUsername = true;
                }

                // User is trying to update their email address, need to 
                // check the email is not already in use
                if (userModel.Email != user.Email)
                {
                    // Add get by email address
                    if (MembershipService.GetUserByEmail(userModel.Email) != null)
                    {
                        Context.RollBack();
                        ModelState.AddModelError(string.Empty,
                            LocalizationService.GetResourceString("Members.Errors.DuplicateEmail"));
                        return View(userModel);
                    }
                    user.Email = userModel.Email;
                }

                MembershipService.ProfileUpdated(user);

                ShowMessage(new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Member.ProfileUpdated"),
                    MessageType = GenericMessages.success
                });

                try
                {
                    Context.SaveChanges();

                    if (changedUsername)
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
                catch (Exception ex)
                {
                    Context.RollBack();
                    LoggingService.Error(ex);
                    ModelState.AddModelError(string.Empty,
                        LocalizationService.GetResourceString("Errors.GenericMessage"));
                }

                return View(userModel);
            }


            return ErrorToHomePage(LocalizationService.GetResourceString("Errors.NoPermission"));
        }

        /// <summary>
        ///     The side admin panel
        /// </summary>
        /// <param name="isDropDown"></param>
        /// <returns></returns>
        [Authorize]
        public PartialViewResult SideAdminPanel(bool isDropDown)
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
        public PartialViewResult AdminMemberProfileTools()
        {
            return PartialView();
        }

        /// <summary>
        ///     Autocomplete
        /// </summary>
        /// <param name="term"></param>
        /// <returns></returns>
        [Authorize]
        public string AutoComplete(string term)
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
        public ActionResult Report(Guid id)
        {
            if (SettingsService.GetSettings().EnableMemberReporting)
            {
                var user = MembershipService.GetUser(id);
                return View(new ReportMemberViewModel {Id = user.Id, Username = user.UserName});
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
        public ActionResult Report(ReportMemberViewModel viewModel)
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

                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Report.ReportSent"),
                    MessageType = GenericMessages.success
                };
                return View(new ReportMemberViewModel {Id = user.Id, Username = user.UserName});
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
        public async Task<ActionResult> Search(int? p, string search)
        {
            var pageIndex = p ?? 1;
            var allUsers = string.IsNullOrWhiteSpace(search)
                ? await MembershipService.GetAll(pageIndex, SiteConstants.Instance.AdminListPageSize)
                : await MembershipService.SearchMembers(search, pageIndex,
                    SiteConstants.Instance.AdminListPageSize);

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
        public PartialViewResult LatestMembersJoined()
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
        public ActionResult ChangePassword()
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
        public ActionResult ChangePassword(ChangePasswordViewModel model)
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
                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
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
        public ActionResult ForgotPassword()
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
        public ActionResult ForgotPassword(ForgotPasswordViewModel forgotPasswordViewModel)
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
                Url.Action("ResetPassword", "Members", new {user.Id, token = user.PasswordResetToken})));

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
        public ViewResult PasswordResetSent()
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
        public ViewResult ResetPassword(Guid? id, string token)
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
        public ActionResult ResetPassword(ResetPasswordViewModel postedModel)
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
        public ViewResult PasswordChanged()
        {
            return View();
        }
    }
}