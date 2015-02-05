using System;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using System.Web.Security;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Enums;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Utilities;
using MVCForum.Website.Application;
using MVCForum.Website.Areas.Admin.ViewModels;
using MVCForum.Website.ViewModels;
using MVCForum.Website.ViewModels.Mapping;
using MembershipCreateStatus = MVCForum.Domain.DomainModel.MembershipCreateStatus;
using MembershipUser = MVCForum.Domain.DomainModel.MembershipUser;

namespace MVCForum.Website.Controllers
{
    public partial class MembersController : BaseController
    {
        private readonly IPostService _postService;
        private readonly IReportService _reportService;
        private readonly IEmailService _emailService;
        private readonly IPrivateMessageService _privateMessageService;
        private readonly IBannedEmailService _bannedEmailService;
        private readonly IBannedWordService _bannedWordService;
        private readonly ITopicNotificationService _topicNotificationService;
        private readonly IPollAnswerService _pollAnswerService;
        private readonly IVoteService _voteService;

        private MembershipUser LoggedOnUser;
        private MembershipRole UsersRole;

        public MembersController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, ILocalizationService localizationService,
            IRoleService roleService, ISettingsService settingsService, IPostService postService, IReportService reportService, IEmailService emailService, IPrivateMessageService privateMessageService, IBannedEmailService bannedEmailService, IBannedWordService bannedWordService, ITopicNotificationService topicNotificationService, IPollAnswerService pollAnswerService, IVoteService voteService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _postService = postService;
            _reportService = reportService;
            _emailService = emailService;
            _privateMessageService = privateMessageService;
            _bannedEmailService = bannedEmailService;
            _bannedWordService = bannedWordService;
            _topicNotificationService = topicNotificationService;
            _pollAnswerService = pollAnswerService;
            _voteService = voteService;

            LoggedOnUser = UserIsAuthenticated ? MembershipService.GetUser(Username) : null;
            UsersRole = LoggedOnUser == null ? RoleService.GetRole(AppConstants.GuestRoleName) : LoggedOnUser.Roles.FirstOrDefault();
        }

        [Authorize(Roles = AppConstants.AdminRoleName)]
        public ActionResult SrubAndBanUser(Guid id)
        {
            var user = MembershipService.GetUser(id);

            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                if (!user.Roles.Any(x => x.RoleName.Contains(AppConstants.AdminRoleName)))
                {
                    MembershipService.ScrubUsers(user, unitOfWork);

                    try
                    {
                        unitOfWork.Commit();
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = LocalizationService.GetResourceString("Members.SuccessfulSrub"),
                            MessageType = GenericMessages.success
                        };
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = LocalizationService.GetResourceString("Members.UnSuccessfulSrub"),
                            MessageType = GenericMessages.danger
                        };
                    }
                }
            }

            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var viewModel = ViewModelMapping.UserToMemberEditViewModel(user);
                viewModel.AllRoles = RoleService.AllRoles();
                return Redirect(user.NiceUrl);
            }

        }

        [Authorize]
        public ActionResult BanMember(Guid id)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var user = MembershipService.GetUser(id);
                var permissions = RoleService.GetPermissions(null, UsersRole);

                if (permissions[AppConstants.PermissionEditMembers].IsTicked)
                {

                    if (!user.Roles.Any(x => x.RoleName.Contains(AppConstants.AdminRoleName)))
                    {
                        user.IsLockedOut = true;

                        try
                        {
                            unitOfWork.Commit();
                            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                            {
                                Message = LocalizationService.GetResourceString("Members.NowBanned"),
                                MessageType = GenericMessages.success
                            };
                        }
                        catch (Exception ex)
                        {
                            unitOfWork.Rollback();
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
        }

        [Authorize]
        public ActionResult UnBanMember(Guid id)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var user = MembershipService.GetUser(id);
                var permissions = RoleService.GetPermissions(null, UsersRole);

                if (permissions[AppConstants.PermissionEditMembers].IsTicked)
                {
                    if (!user.Roles.Any(x => x.RoleName.Contains(AppConstants.AdminRoleName)))
                    {
                        user.IsLockedOut = false;

                        try
                        {
                            unitOfWork.Commit();
                            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                            {
                                Message = LocalizationService.GetResourceString("Members.NowUnBanned"),
                                MessageType = GenericMessages.success
                            };
                        }
                        catch (Exception ex)
                        {
                            unitOfWork.Rollback();
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
        }


        [ChildActionOnly]
        public PartialViewResult GetCurrentActiveMembers()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var viewModel = new ActiveMembersViewModel
                {
                    ActiveMembers = MembershipService.GetActiveMembers()
                };
                return PartialView(viewModel);
            }
        }

        public JsonResult LastActiveCheck()
        {
            if (UserIsAuthenticated)
            {
                var rightNow = DateTime.UtcNow;
                var usersDate = LoggedOnUser.LastActivityDate ?? DateTime.UtcNow.AddDays(-1);

                var span = rightNow.Subtract(usersDate);
                var totalMins = span.TotalMinutes;

                if (totalMins > AppConstants.TimeSpanInMinutesToDoCheck)
                {
                    using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                    {
                        // Update users last activity date so we can show the latest users online
                        LoggedOnUser.LastActivityDate = DateTime.UtcNow;

                        // Update
                        try
                        {
                            unitOfWork.Commit();
                        }
                        catch (Exception ex)
                        {
                            unitOfWork.Rollback();
                            LoggingService.Error(ex);
                        }
                    }
                }
            }

            // You can return anything to reset the timer.
            return Json(new { Timer = "reset" }, JsonRequestBehavior.AllowGet);
        }

        public ActionResult GetByName(string slug)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var member = MembershipService.GetUserBySlug(slug);
                var loggedonId = UserIsAuthenticated ? LoggedOnUser.Id : Guid.Empty;
                var permissions = RoleService.GetPermissions(null, UsersRole);
                return View(new ViewMemberViewModel
                {
                    User = member,
                    LoggedOnUserId = loggedonId,
                    Permissions = permissions
                });
            }
        }

        /// <summary>
        /// Add a new user
        /// </summary>
        /// <returns></returns>
        public ActionResult Register()
        {
            if (SettingsService.GetSettings().SuspendRegistration != true)
            {
                using (UnitOfWorkManager.NewUnitOfWork())
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
                    if (!string.IsNullOrEmpty(returnUrl))
                    {
                        viewModel.ReturnUrl = returnUrl;
                    }

                    return View(viewModel);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Add a new user
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Register(MemberAddViewModel userModel)
        {
            if (SettingsService.GetSettings().SuspendRegistration != true)
            {
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    // First see if there is a spam question and if so, the answer matches
                    if (!string.IsNullOrEmpty(SettingsService.GetSettings().SpamQuestion))
                    {
                        // There is a spam question, if answer is wrong return with error
                        if (userModel.SpamAnswer == null || userModel.SpamAnswer.Trim() != SettingsService.GetSettings().SpamAnswer)
                        {
                            // POTENTIAL SPAMMER!
                            ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Error.WrongAnswerRegistration"));
                            return View();
                        }
                    }

                    // Secondly see if the email is banned
                    if (_bannedEmailService.EmailIsBanned(userModel.Email))
                    {
                        ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Error.EmailIsBanned"));
                        return View();
                    }
                }

                // Standard Login
                userModel.LoginType = LoginType.Standard;

                // Do the register logic
                return MemberRegisterLogic(userModel);

            }
            return RedirectToAction("Index", "Home");
        }

        public ActionResult SocialLoginValidator()
        {
            // Store the viewModel in TempData - Which we'll use in the register logic
            if (TempData[AppConstants.MemberRegisterViewModel] != null)
            {
                var userModel = (TempData[AppConstants.MemberRegisterViewModel] as MemberAddViewModel);

                // Do the register logic
                return MemberRegisterLogic(userModel);
            }

            ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
            return View("Register");
        }

        public ActionResult MemberRegisterLogic(MemberAddViewModel userModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var userToSave = new MembershipUser
                {
                    UserName = _bannedWordService.SanitiseBannedWords(userModel.UserName),
                    Email = userModel.Email,
                    Password = userModel.Password,
                    IsApproved = userModel.IsApproved,
                    Comment = userModel.Comment,
                };

                var homeRedirect = false;

                // Now check settings, see if users need to be manually authorised
                // OR Does the user need to confirm their email
                var manuallyAuthoriseMembers = SettingsService.GetSettings().ManuallyAuthoriseNewMembers;
                var memberEmailAuthorisationNeeded = SettingsService.GetSettings().NewMemberEmailConfirmation ?? false;
                if (manuallyAuthoriseMembers || memberEmailAuthorisationNeeded)
                {
                    userToSave.IsApproved = false;
                }

                var createStatus = MembershipService.CreateUser(userToSave);
                if (createStatus != MembershipCreateStatus.Success)
                {
                    ModelState.AddModelError(string.Empty, MembershipService.ErrorCodeToString(createStatus));
                }
                else
                {
                    // See if this is a social login and we have their profile pic
                    if (!string.IsNullOrEmpty(userModel.SocialProfileImageUrl))
                    {
                        // We have an image url - Need to save it to their profile
                        var image = AppHelpers.GetImageFromExternalUrl(userModel.SocialProfileImageUrl);

                        // Set upload directory - Create if it doesn't exist
                        var uploadFolderPath = HostingEnvironment.MapPath(string.Concat(SiteConstants.UploadFolderPath, userToSave.Id));
                        if (!Directory.Exists(uploadFolderPath))
                        {
                            Directory.CreateDirectory(uploadFolderPath);
                        }

                        // Get the file name
                        var fileName = Path.GetFileName(userModel.SocialProfileImageUrl);

                        // Create a HttpPostedFileBase image from the C# Image
                        using (var stream = new MemoryStream())
                        {
                            image.Save(stream, ImageFormat.Jpeg);
                            stream.Position = 0;
                            HttpPostedFileBase formattedImage = new MemoryFile(stream, "image/jpeg", fileName);

                            // Upload the file
                            var uploadResult = AppHelpers.UploadFile(formattedImage, uploadFolderPath, LocalizationService);

                            // Don't throw error if problem saving avatar, just don't save it.
                            if (uploadResult.UploadSuccessful)
                            {
                                userToSave.Avatar = uploadResult.UploadedFileName;
                            }
                        }

                    }

                    // Store access token for social media account in case we want to do anything with it
                    if (userModel.LoginType == LoginType.Facebook)
                    {
                        userToSave.FacebookAccessToken = userModel.UserAccessToken;
                    }
                    if (userModel.LoginType == LoginType.Google)
                    {
                        userToSave.GoogleAccessToken = userModel.UserAccessToken;
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

                        unitOfWork.Commit();

                        if (homeRedirect)
                        {
                            if (Url.IsLocalUrl(userModel.ReturnUrl) && userModel.ReturnUrl.Length > 1 && userModel.ReturnUrl.StartsWith("/")
                            && !userModel.ReturnUrl.StartsWith("//") && !userModel.ReturnUrl.StartsWith("/\\"))
                            {
                                return Redirect(userModel.ReturnUrl);
                            }
                            return RedirectToAction("Index", "Home", new { area = string.Empty });
                        }
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        FormsAuthentication.SignOut();
                        ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
                    }
                }
            }

            return View("Register");
        }

        private void SetRegisterViewBagMessage(bool manuallyAuthoriseMembers, bool memberEmailAuthorisationNeeded, MembershipUser userToSave)
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
                FormsAuthentication.SetAuthCookie(userToSave.UserName, false);
                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Members.NowRegistered"),
                    MessageType = GenericMessages.success
                };
            }
        }

        private void SendEmailConfirmationEmail(MembershipUser userToSave)
        {
            var manuallyAuthoriseMembers = SettingsService.GetSettings().ManuallyAuthoriseNewMembers;
            var memberEmailAuthorisationNeeded = SettingsService.GetSettings().NewMemberEmailConfirmation ?? false;
            if (manuallyAuthoriseMembers == false && memberEmailAuthorisationNeeded)
            {
                if (!string.IsNullOrEmpty(userToSave.Email))
                {
                    // SEND AUTHORISATION EMAIL
                    var sb = new StringBuilder();
                    var confirmationLink = string.Concat(StringUtils.ReturnCurrentDomain(), Url.Action("EmailConfirmation", new { id = userToSave.Id }));
                    sb.AppendFormat("<p>{0}</p>", string.Format(LocalizationService.GetResourceString("Members.MemberEmailAuthorisation.EmailBody"),
                                                SettingsService.GetSettings().ForumName,
                                                string.Format("<p><a href=\"{0}\">{0}</a></p>", confirmationLink)));
                    var email = new Email
                    {
                        EmailTo = userToSave.Email,
                        NameTo = userToSave.UserName,
                        Subject = LocalizationService.GetResourceString("Members.MemberEmailAuthorisation.Subject")
                    };
                    email.Body = _emailService.EmailTemplate(email.NameTo, sb.ToString());
                    _emailService.SendMail(email);

                    // ADD COOKIE
                    // We add a cookie for 7 days, which will display the resend email confirmation button
                    // This cookie is removed when they click the confirmation link
                    var myCookie = new HttpCookie(AppConstants.MemberEmailConfirmationCookieName)
                    {
                        Value = string.Format("{0}#{1}", userToSave.Email, userToSave.UserName),
                        Expires = DateTime.UtcNow.AddDays(7)
                    };
                    // Add the cookie.
                    Response.Cookies.Add(myCookie);
                }
            }
        }

        public ActionResult ResendEmailConfirmation(string username)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var user = MembershipService.GetUser(username);
                if (user != null)
                {
                    SendEmailConfirmationEmail(user);
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = LocalizationService.GetResourceString("Members.MemberEmailAuthorisationNeeded"),
                        MessageType = GenericMessages.success
                    };
                }

                try
                {
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                }
            }
            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Email confirmation page from the link in the users email
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public ActionResult EmailConfirmation(Guid id)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                // Checkconfirmation
                var user = MembershipService.GetUser(id);
                if (user != null)
                {
                    // Set the user to active
                    user.IsApproved = true;

                    // Delete Cookie and log them in if this cookie is present
                    if (Request.Cookies[AppConstants.MemberEmailConfirmationCookieName] != null)
                    {
                        var myCookie = new HttpCookie(AppConstants.MemberEmailConfirmationCookieName)
                        {
                            Expires = DateTime.UtcNow.AddDays(-1)
                        };
                        Response.Cookies.Add(myCookie);

                        // Login code
                        FormsAuthentication.SetAuthCookie(user.UserName, false);
                    }

                    // Show a new message
                    // We use temp data because we are doing a redirect
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = LocalizationService.GetResourceString("Members.NowApproved"),
                        MessageType = GenericMessages.success
                    };
                }

                try
                {
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                }
            }

            return RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// Log on
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOn()
        {
            // Create the empty view model
            var viewModel = new LogOnViewModel();

            // See if a return url is present or not and add it
            var returnUrl = Request["ReturnUrl"];
            if (!string.IsNullOrEmpty(returnUrl))
            {
                viewModel.ReturnUrl = returnUrl;
            }

            return View(viewModel);
        }

        /// <summary>
        /// Log on post
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOn(LogOnViewModel model)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var username = model.UserName;
                var password = model.Password;

                try
                {
                    if (ModelState.IsValid)
                    {
                        var message = new GenericMessageViewModel();
                        var user = new MembershipUser();
                        if (MembershipService.ValidateUser(username, password, System.Web.Security.Membership.MaxInvalidPasswordAttempts))
                        {
                            // Set last login date
                            user = MembershipService.GetUser(username);
                            if (user.IsApproved && !user.IsLockedOut)
                            {
                                FormsAuthentication.SetAuthCookie(username, model.RememberMe);
                                user.LastLoginDate = DateTime.UtcNow;

                                if (Url.IsLocalUrl(model.ReturnUrl) && model.ReturnUrl.Length > 1 && model.ReturnUrl.StartsWith("/")
                                    && !model.ReturnUrl.StartsWith("//") && !model.ReturnUrl.StartsWith("/\\"))
                                {
                                    return Redirect(model.ReturnUrl);
                                }

                                message.Message = LocalizationService.GetResourceString("Members.NowLoggedIn");
                                message.MessageType = GenericMessages.success;

                                return RedirectToAction("Index", "Home", new { area = string.Empty });
                            }
                            //else if (!user.IsApproved && SettingsService.GetSettings().ManuallyAuthoriseNewMembers)
                            //{

                            //    message.Message = LocalizationService.GetResourceString("Members.NowRegisteredNeedApproval");
                            //    message.MessageType = GenericMessages.success;

                            //}
                            //else if (!user.IsApproved && SettingsService.GetSettings().NewMemberEmailConfirmation == true)
                            //{

                            //    message.Message = LocalizationService.GetResourceString("Members.MemberEmailAuthorisationNeeded");
                            //    message.MessageType = GenericMessages.success;
                            //}
                        }

                        // Only show if we have something to actually show to the user
                        if (!string.IsNullOrEmpty(message.Message))
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
                                    ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Members.Errors.PasswordIncorrect"));
                                    break;

                                case LoginAttemptStatus.PasswordAttemptsExceeded:
                                    ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Members.Errors.PasswordAttemptsExceeded"));
                                    break;

                                case LoginAttemptStatus.UserLockedOut:
                                    ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Members.Errors.UserLockedOut"));
                                    break;

                                case LoginAttemptStatus.UserNotApproved:
                                    ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Members.Errors.UserNotApproved"));
                                    user = MembershipService.GetUser(username);
                                    SendEmailConfirmationEmail(user);
                                    break;

                                default:
                                    ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Members.Errors.LogonGeneric"));
                                    break;
                            }
                        }
                    }
                }

                finally
                {
                    try
                    {
                        unitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                    }

                }

                return View(model);
            }
        }

        /// <summary>
        /// Get: log off user
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOff()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                FormsAuthentication.SignOut();
                ViewBag.Message = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Members.NowLoggedOut"),
                    MessageType = GenericMessages.success
                };
                return RedirectToAction("Index", "Home", new { area = string.Empty });
            }
        }

        [HttpPost]
        public PartialViewResult GetMemberDiscussions(Guid Id)
        {
            if (Request.IsAjaxRequest())
            {
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    // Get the user discussions, only grab 100 posts
                    var posts = _postService.GetByMember(Id, 100);

                    // Get the distinct topics
                    var topics = posts.Select(x => x.Topic).Distinct().Take(6).OrderByDescending(x => x.LastPost.DateCreated).ToList();

                    // Get the Topic View Models
                    var topicViewModels = ViewModelMapping.CreateTopicViewModels(topics, RoleService, UsersRole, LoggedOnUser, SettingsService.GetSettings());

                    // create the view model
                    var viewModel = new ViewMemberDiscussionsViewModel
                    {
                        Topics = topicViewModels
                    };


                    return PartialView(viewModel);
                }
            }
            return null;
        }

        private MemberFrontEndEditViewModel PopulateMemberViewModel(MembershipUser user)
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
                Avatar = user.Avatar
            };
            return viewModel;
        }

        [Authorize]
        public ActionResult Edit(Guid id)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var loggedOnUserId = (LoggedOnUser != null ? LoggedOnUser.Id : Guid.Empty);

                var permissions = RoleService.GetPermissions(null, UsersRole);

                // Check is has permissions
                if (User.IsInRole(AppConstants.AdminRoleName) || loggedOnUserId == id || permissions[AppConstants.PermissionEditMembers].IsTicked)
                {
                    var user = MembershipService.GetUser(id);
                    var viewModel = PopulateMemberViewModel(user);

                    return View(viewModel);
                }

                return ErrorToHomePage(LocalizationService.GetResourceString("Errors.NoPermission"));
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult Edit(MemberFrontEndEditViewModel userModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var loggedOnUserId = (LoggedOnUser != null ? LoggedOnUser.Id : Guid.Empty);
                var permissions = RoleService.GetPermissions(null, UsersRole);

                // Check is has permissions
                if (User.IsInRole(AppConstants.AdminRoleName) || loggedOnUserId == userModel.Id || permissions[AppConstants.PermissionEditMembers].IsTicked)
                {
                    // Get the user from DB
                    var user = MembershipService.GetUser(userModel.Id);

                    // Before we do anything - Check stop words
                    var stopWords = _bannedWordService.GetAll(true);
                    var bannedWords = _bannedWordService.GetAll().Select(x => x.Word).ToList();

                    // Check the fields for bad words
                    foreach (var stopWord in stopWords)
                    {
                        if ((userModel.Facebook != null && userModel.Facebook.IndexOf(stopWord.Word, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                            (userModel.Location != null && userModel.Location.IndexOf(stopWord.Word, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                            (userModel.Signature != null && userModel.Signature.IndexOf(stopWord.Word, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                            (userModel.Twitter != null && userModel.Twitter.IndexOf(stopWord.Word, StringComparison.CurrentCultureIgnoreCase) >= 0) ||
                            (userModel.Website != null && userModel.Website.IndexOf(stopWord.Word, StringComparison.CurrentCultureIgnoreCase) >= 0))
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

                    // Sort image out first
                    if (userModel.Files != null)
                    {
                        // Before we save anything, check the user already has an upload folder and if not create one
                        var uploadFolderPath = HostingEnvironment.MapPath(string.Concat(SiteConstants.UploadFolderPath, LoggedOnUser.Id));
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

                    user.Age = userModel.Age;
                    user.Facebook = _bannedWordService.SanitiseBannedWords(userModel.Facebook, bannedWords);
                    user.Location = _bannedWordService.SanitiseBannedWords(userModel.Location, bannedWords);
                    user.Signature = _bannedWordService.SanitiseBannedWords(StringUtils.ScrubHtml(userModel.Signature), bannedWords);
                    user.Twitter = _bannedWordService.SanitiseBannedWords(userModel.Twitter, bannedWords);
                    user.Website = _bannedWordService.SanitiseBannedWords(userModel.Website, bannedWords);

                    // If there is a location try and save the longitude and latitude
                    if (!string.IsNullOrEmpty(user.Location))
                    {
                        try
                        {
                            var longLat = LocalisationUtils.GeocodeGoogle(user.Location);
                            if (longLat != null && longLat[0] != "0")
                            {
                                // Got the long lat and save them to the user
                                user.Latitude = longLat[0];
                                user.Longitude = longLat[1];
                            }
                        }
                        catch
                        {
                            LoggingService.Error("Error getting longitude and latitude from location");
                        }
                    }

                    // User is trying to change username, need to check if a user already exists
                    // with the username they are trying to change to
                    var changedUsername = false;
                    var sanitisedUsername = _bannedWordService.SanitiseBannedWords(userModel.UserName, bannedWords);
                    if (sanitisedUsername != user.UserName)
                    {
                        if (MembershipService.GetUser(sanitisedUsername) != null)
                        {
                            unitOfWork.Rollback();
                            ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Members.Errors.DuplicateUserName"));
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
                            unitOfWork.Rollback();
                            ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Members.Errors.DuplicateEmail"));
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
                        unitOfWork.Commit();

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
                                    var newFormsIdentity = new FormsIdentity(new FormsAuthenticationTicket(authTicket.Version,
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
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
                    }

                    return View(userModel);
                }


                return ErrorToHomePage(LocalizationService.GetResourceString("Errors.NoPermission"));
            }
        }

        [Authorize]
        public PartialViewResult SideAdminPanel()
        {
            var count = 0;
            if (LoggedOnUser != null)
            {
                count = _privateMessageService.NewPrivateMessageCount(LoggedOnUser.Id);
            }

            if (count > 0)
            {
                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Member.HasNewPrivateMessages"),
                    MessageType = GenericMessages.info
                };
            }
            return PartialView(new ViewAdminSidePanelViewModel { CurrentUser = LoggedOnUser, NewPrivateMessageCount = count });
        }

        public PartialViewResult AdminMemberProfileTools()
        {
            return PartialView();
        }

        [Authorize]
        public string AutoComplete(string term)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                if (!string.IsNullOrEmpty(term))
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
        }

        [Authorize]
        public ActionResult Report(Guid id)
        {
            if (SettingsService.GetSettings().EnableMemberReporting)
            {
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    var user = MembershipService.GetUser(id);
                    return View(new ReportMemberViewModel { Id = user.Id, Username = user.UserName });
                }
            }
            return ErrorToHomePage(LocalizationService.GetResourceString("Errors.GenericMessage"));
        }

        [HttpPost]
        [Authorize]
        public ActionResult Report(ReportMemberViewModel viewModel)
        {
            if (SettingsService.GetSettings().EnableMemberReporting)
            {
                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    var user = MembershipService.GetUser(viewModel.Id);
                    var report = new Report
                                     {
                                         Reason = viewModel.Reason,
                                         ReportedMember = user,
                                         Reporter = LoggedOnUser
                                     };
                    _reportService.MemberReport(report);

                    try
                    {
                        unitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                    }

                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = LocalizationService.GetResourceString("Report.ReportSent"),
                        MessageType = GenericMessages.success
                    };
                    return View(new ReportMemberViewModel { Id = user.Id, Username = user.UserName });
                }
            }
            return ErrorToHomePage(LocalizationService.GetResourceString("Errors.GenericMessage"));
        }

        public ActionResult Search(int? p, string search)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var pageIndex = p ?? 1;
                var allUsers = string.IsNullOrEmpty(search) ? MembershipService.GetAll(pageIndex, SiteConstants.AdminListPageSize) :
                                    MembershipService.SearchMembers(search, pageIndex, SiteConstants.AdminListPageSize);

                // Redisplay list of users
                var allViewModelUsers = allUsers.Select(user => new PublicSingleMemberListViewModel
                                                                    {
                                                                        UserName = user.UserName,
                                                                        NiceUrl = user.NiceUrl,
                                                                        CreateDate = user.CreateDate,
                                                                        TotalPoints = user.TotalPoints,
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
        }

        [ChildActionOnly]
        public PartialViewResult LatestMembersJoined()
        {
            var viewModel = new ListLatestMembersViewModel();
            var users = MembershipService.GetLatestUsers(10).ToDictionary(o => o.UserName, o => o.NiceUrl);
            viewModel.Users = users;
            return PartialView(viewModel);
        }

        [Authorize]
        public ActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult ChangePassword(ChangePasswordViewModel model)
        {
            var changePasswordSucceeded = true;
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                if (ModelState.IsValid)
                {
                    // ChangePassword will throw an exception rather than return false in certain failure scenarios.
                    changePasswordSucceeded = MembershipService.ChangePassword(LoggedOnUser, model.OldPassword, model.NewPassword);

                    try
                    {
                        unitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        changePasswordSucceeded = false;
                    }
                }
            }

            // Commited successfully carry on
            using (UnitOfWorkManager.NewUnitOfWork())
            {
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

        }

        public ActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ForgotPassword(ForgotPasswordViewModel forgotPasswordViewModel)
        {
            var changePasswordSucceeded = true;
            var currentUser = new MembershipUser();
            var newPassword = StringUtils.RandomString(8);

            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                if (ModelState.IsValid)
                {
                    currentUser = MembershipService.GetUserByEmail(forgotPasswordViewModel.EmailAddress);
                    if (currentUser != null)
                    {
                        changePasswordSucceeded = MembershipService.ResetPassword(currentUser, newPassword);

                        try
                        {
                            unitOfWork.Commit();
                        }
                        catch (Exception ex)
                        {
                            unitOfWork.Rollback();
                            LoggingService.Error(ex);
                            changePasswordSucceeded = false;
                        }
                    }
                    else
                    {
                        changePasswordSucceeded = false;
                    }
                }
            }

            // Success send newpassword to the user telling them password has been changed
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {

                if (changePasswordSucceeded)
                {
                    var sb = new StringBuilder();
                    sb.AppendFormat("<p>{0}</p>", string.Format(LocalizationService.GetResourceString("Members.ForgotPassword.Email"), SettingsService.GetSettings().ForumName));
                    sb.AppendFormat("<p><b>{0}</b></p>", newPassword);
                    var email = new Email
                                    {
                                        EmailTo = currentUser.Email,
                                        NameTo = currentUser.UserName,
                                        Subject = LocalizationService.GetResourceString("Members.ForgotPassword.Subject")
                                    };
                    email.Body = _emailService.EmailTemplate(email.NameTo, sb.ToString());
                    _emailService.SendMail(email);

                    try
                    {
                        unitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                    }

                    // We use temp data because we are doing a redirect
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = LocalizationService.GetResourceString("Members.ForgotPassword.SuccessMessage"),
                        MessageType = GenericMessages.success
                    };
                    return View();
                }

                ModelState.AddModelError("", LocalizationService.GetResourceString("Members.ForgotPassword.ErrorMessage"));
                return View(forgotPasswordViewModel);
            }
        }

    }
}
