using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
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

        //#region Common Methods

        //private bool ProcessSocialLogonUser(MembershipUser user, bool doCommit)
        //{
        //    // Secondly see if the email is banned
        //    if (_bannedEmailService.EmailIsBanned(user.Email))
        //    {
        //        doCommit = false;
        //        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
        //        {
        //            Message = LocalizationService.GetResourceString("Error.EmailIsBanned"),
        //            MessageType = GenericMessages.error
        //        };
        //    }
        //    else
        //    {
        //        // Check not already someone with that user name, if so append count
        //        var exists = MembershipService.GetUser(user.UserName);
        //        if (exists != null)
        //        {
        //            var howMany = MembershipService.SearchMembers(user.UserName, int.MaxValue);
        //            user.UserName = string.Format("{0} ({1})", user.UserName, howMany != null ? howMany.Count : 1);
        //        }

        //        // Now check settings, see if users need to be manually authorised
        //        var manuallyAuthoriseMembers = SettingsService.GetSettings().ManuallyAuthoriseNewMembers;
        //        var memberEmailAuthorisationNeeded = SettingsService.GetSettings().NewMemberEmailConfirmation ?? false;
        //        if (manuallyAuthoriseMembers || memberEmailAuthorisationNeeded)
        //        {
        //            user.IsApproved = false;
        //        }

        //        var createStatus = MembershipService.CreateUser(user);
        //        if (createStatus != MembershipCreateStatus.Success)
        //        {
        //            doCommit = false;
        //            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
        //            {
        //                Message = MembershipService.ErrorCodeToString(createStatus),
        //                MessageType = GenericMessages.error
        //            };
        //        }
        //        else
        //        {
        //            // Set the view bag message here
        //            SetRegisterViewBagMessage(manuallyAuthoriseMembers, memberEmailAuthorisationNeeded, user);
        //        }
        //    }

        //    return doCommit;
        //}

        //#endregion

        //#region Social Logons

        //private void InstantiateTwitterInMemoryTokenManager()
        //{
        //    var twitterAppId = ConfigUtils.GetAppSetting("TwitterAppId");
        //    var twitterAppSecret = ConfigUtils.GetAppSetting("TwitterAppSecret");

        //    // Only instantiate if the twitter credentials are not null
        //    if (!string.IsNullOrEmpty(twitterAppId) && !string.IsNullOrEmpty(twitterAppSecret))
        //    {
        //        _tokenManager = new InMemoryTokenManager(twitterAppId, twitterAppSecret);
        //    }
        //}

        //public ActionResult LogonTwitter()
        //{
        //    InstantiateTwitterInMemoryTokenManager();
        //    var client = new TwitterClient(_tokenManager, Url.Action("TwitterCallback"));
        //    client.StartAuthentication();
        //    return null;
        //}

        //public ActionResult TwitterCallback()
        //{
        //    InstantiateTwitterInMemoryTokenManager();

        //    var client = new TwitterClient(_tokenManager, Url.Action("TwitterCallback"));

        //    if (client.FinishAuthentication())
        //    {
        //        // Boom we are in, get the stuff we need
        //        //client.UserName;
        //        //client.AccessToken;
        //        //client.SecretToken;

        //        using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
        //        {
        //            var doCommit = true;

        //            // See if the user has already logged in to this site using open Id
        //            var user = MembershipService.GetUserByTwitterId(client.AccessToken);
        //            var fakeEmail = string.Format("{0}@twitter.com", client.UserName);

        //            if (user == null)
        //            {
        //                // First time logging in, so need to register them as new user
        //                // password is irrelavant as they'll login using FB Id so generate random one

        //                user = new MembershipUser
        //                {
        //                    // Bit shit, but twitter won't give you an email. So we do this and
        //                    // Set notifications to false.
        //                    Email = fakeEmail,
        //                    Password = StringUtils.RandomString(8),
        //                    TwitterAccessToken = client.AccessToken,
        //                    IsExternalAccount = true,
        //                    DisableEmailNotifications = true,
        //                    UserName = _bannedWordService.SanitiseBannedWords(client.UserName),
        //                    Twitter = string.Format("http://twitter.com/{0}", _bannedWordService.SanitiseBannedWords(client.UserName))
        //                };

        //                doCommit = ProcessSocialLogonUser(user, doCommit);

        //            }
        //            else
        //            {
        //                // Do an update to make sure we have the most recent details
        //                user.Email = fakeEmail;
        //                user.MiscAccessToken = client.AccessToken;

        //                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
        //                {
        //                    Message = LocalizationService.GetResourceString("Members.NowLoggedIn"),
        //                    MessageType = GenericMessages.success
        //                };

        //                // Log the user in
        //                FormsAuthentication.SetAuthCookie(user.UserName, true);
        //            }

        //            if (doCommit)
        //            {
        //                try
        //                {
        //                    unitOfWork.Commit();
        //                    // Only send the email if the admin is not manually authorising emails or it's pointless
        //                    // CAN'T SENT TO TWITTER USERS AS WE DON'T HAVE AN EMAIL! :(
        //                    // https://dev.twitter.com/discussions/1737
        //                    //SendEmailConfirmationEmail(user);
        //                    return RedirectToAction("Index", "Home");
        //                }
        //                catch (Exception ex)
        //                {
        //                    unitOfWork.Rollback();
        //                    LoggingService.Error(ex);
        //                    FormsAuthentication.SignOut();
        //                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
        //                    {
        //                        Message = LocalizationService.GetResourceString("Errors.GenericMessage"),
        //                        MessageType = GenericMessages.error
        //                    };

        //                }
        //            }

        //        }

        //    }

        //    // Only add this if one hasn't been added already
        //    if (TempData[AppConstants.MessageViewBagName] == null)
        //    {
        //        // Either cancelled or there was an error
        //        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
        //        {
        //            Message = LocalizationService.GetResourceString("Errors.GenericMessage"),
        //            MessageType = GenericMessages.error
        //        };
        //    }
        //    return RedirectToAction("LogOn", "Members");
        //}



        //public ActionResult LogonFacebook()
        //{
        //    var client = new FacebookClient
        //    {
        //        ClientIdentifier = ConfigUtils.GetAppSetting("FacebookAppId"),
        //        ClientCredentialApplicator = ClientCredentialApplicator.PostParameter(ConfigUtils.GetAppSetting("FacebookAppSecret"))
        //    };

        //    var authorization = client.ProcessUserAuthorization();
        //    if (authorization == null)
        //    {
        //        // Kick off authorization request
        //        client.RequestUserAuthorization(client.ScopeParameters);
        //    }
        //    else
        //    {

        //        if (authorization.AccessToken == null)
        //        {
        //            // User has cancelled so just redirect to home page
        //            return RedirectToAction("Index", "Home");
        //        }

        //        var request = WebRequest.Create(string.Concat("https://graph.facebook.com/me?access_token=", Uri.EscapeDataString(authorization.AccessToken)));
        //        using (var response = request.GetResponse())
        //        {
        //            using (var responseStream = response.GetResponseStream())
        //            {
        //                var fbModel = FacebookModel.Deserialize(responseStream);

        //                // use the data in the graph object to authorise the user
        //                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
        //                {
        //                    var doCommit = true;
        //                    // First thing check if this user has already registered using facebook before
        //                    // Get the user by their FB Id
        //                    var fbUser = MembershipService.GetUserByFacebookId(fbModel.Id);

        //                    if (fbUser == null)
        //                    {
        //                        // First time logging in, so need to register them as new user
        //                        // password is irrelavant as they'll login using FB Id so generate random one
        //                        fbUser = new MembershipUser
        //                        {
        //                            UserName = _bannedWordService.SanitiseBannedWords(fbModel.Name),
        //                            Email = fbModel.Email,
        //                            Password = StringUtils.RandomString(8),
        //                            FacebookId = fbModel.Id,
        //                            FacebookAccessToken = authorization.AccessToken,
        //                            IsExternalAccount = true
        //                        };

        //                        doCommit = ProcessSocialLogonUser(fbUser, doCommit);

        //                    }
        //                    else
        //                    {
        //                        // Do an update to make sure we have the most recent details
        //                        fbUser.Email = fbModel.Email;
        //                        fbUser.FacebookAccessToken = authorization.AccessToken;

        //                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
        //                        {
        //                            Message = LocalizationService.GetResourceString("Members.NowLoggedIn"),
        //                            MessageType = GenericMessages.success
        //                        };

        //                        // Log the user in
        //                        FormsAuthentication.SetAuthCookie(fbUser.UserName, true);
        //                    }

        //                    if (doCommit)
        //                    {
        //                        try
        //                        {
        //                            unitOfWork.Commit();
        //                            SendEmailConfirmationEmail(fbUser);
        //                            return RedirectToAction("Index", "Home");
        //                        }
        //                        catch (Exception ex)
        //                        {
        //                            unitOfWork.Rollback();
        //                            LoggingService.Error(ex);
        //                            FormsAuthentication.SignOut();
        //                            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
        //                            {
        //                                Message = LocalizationService.GetResourceString("Errors.GenericMessage"),
        //                                MessageType = GenericMessages.error
        //                            };

        //                        }
        //                    }

        //                }
        //            }
        //        }
        //    }
        //    // Only add this if one hasn't been added already
        //    if (TempData[AppConstants.MessageViewBagName] == null)
        //    {
        //        // Either cancelled or there was an error
        //        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
        //        {
        //            Message = LocalizationService.GetResourceString("Errors.GenericMessage"),
        //            MessageType = GenericMessages.error
        //        };
        //    }
        //    return RedirectToAction("LogOn");
        //}

        //public ActionResult LogonGoogle(string returnUrl)
        //{
        //    var client = new GooglePlusClient {
        //        ClientIdentifier = ConfigUtils.GetAppSetting("GooglePlusAppId"),
        //        ClientCredentialApplicator = ClientCredentialApplicator.PostParameter(ConfigUtils.GetAppSetting("GooglePlusAppSecret"))
        //    };

        //    var authorization = client.ProcessUserAuthorization();
        //    if (authorization == null) {
        //        client.RequestUserAuthorization(client.ScopeParameters);
        //    } else {
        //        if (authorization.AccessToken == null) {
        //            return RedirectToAction("Index", "Home");
        //        }

        //        var request = WebRequest.Create(string.Concat("https://www.googleapis.com/oauth2/v1/tokeninfo?access_token=", Uri.EscapeDataString(authorization.AccessToken)));
        //        using (var response = request.GetResponse()) {
        //            using (var responseStream = response.GetResponseStream()) {
        //                var gpModel = GooglePlusModel.Deserialize(responseStream);

        //                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork()) {
        //                    var doCommit = true;
        //                    var gpUser = MembershipService.GetUserByEmail(gpModel.Email);

        //                    if (gpUser == null) {
        //                        gpUser = new MembershipUser {
        //                            UserName = _bannedWordService.SanitiseBannedWords(gpModel.Name),
        //                            Email = gpModel.Email,
        //                            Password = StringUtils.RandomString(8),
        //                            GoogleId = gpModel.Id,
        //                            GoogleAccessToken = authorization.AccessToken,
        //                            IsExternalAccount = true
        //                        };

        //                        doCommit = ProcessSocialLogonUser(gpUser, doCommit);
        //                    } else {
        //                        gpUser.Email = gpModel.Email;
        //                        gpUser.GoogleAccessToken = authorization.AccessToken;

        //                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel {
        //                            Message = LocalizationService.GetResourceString("Members.NowLoggedIn"),
        //                            MessageType = GenericMessages.success
        //                        };

        //                        FormsAuthentication.SetAuthCookie(gpUser.UserName, true);
        //                    }

        //                    if (doCommit) {
        //                        try {
        //                            unitOfWork.Commit();
        //                            SendEmailConfirmationEmail(gpUser);
        //                            return RedirectToAction("Index", "Home");
        //                        } catch (Exception ex) {
        //                            unitOfWork.Rollback();
        //                            LoggingService.Error(ex);
        //                            FormsAuthentication.SignOut();
        //                            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel {
        //                                Message = LocalizationService.GetResourceString("Errors.GenericMessage"),
        //                                MessageType = GenericMessages.error
        //                            };
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }

        //    if (TempData[AppConstants.MessageViewBagName] == null) {
        //        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel {
        //            Message = LocalizationService.GetResourceString("Errors.GenericMessage"),
        //            MessageType = GenericMessages.error
        //        };
        //    }

        //    return RedirectToAction("Logon");
 
        //}

        //public ActionResult LogonYahoo(string returnUrl)
        //{
        //    var response = OpenAuthHelpers.CheckOpenIdResponse();

        //    // If this is null we haven't gone off to the providers request permission page yet
        //    if (response == null)
        //    {
        //        // Set the request to the specific provider
        //        var request = OpenAuthHelpers.GetRedirectActionRequest(WellKnownProviders.Yahoo);

        //        // Redirect to the providers login page and asks user for permission to share the profile fields requested.
        //        return request.RedirectingResponse.AsActionResultMvc5();
        //    }

        //    // If we get here then we have been to the provider page and been redirected back here
        //    switch (response.Status)
        //    {
        //        case AuthenticationStatus.Authenticated:
        //            // Woot! All good in the hood - User has authorised us

        //            // Get the identifier from the provider
        //            var oid = response.ClaimedIdentifier.ToString();

        //            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
        //            {
        //                var doCommit = true;

        //                // See if the user has already logged in to this site using open Id
        //                var user = MembershipService.GetUserByOpenIdToken(oid);
        //                var fetch = response.GetExtension<FetchResponse>();
        //                if (user == null)
        //                {
        //                    // First time logging in, so need to register them as new user
        //                    // password is irrelavant as they'll login using FB Id so generate random one

        //                    user = new MembershipUser
        //                    {
        //                        Email = fetch.GetAttributeValue(WellKnownAttributes.Contact.Email),
        //                        Password = StringUtils.RandomString(8),
        //                        MiscAccessToken = oid,
        //                        IsExternalAccount = true,
        //                    };
        //                    user.UserName = _bannedWordService.SanitiseBannedWords(user.Email);

        //                    doCommit = ProcessSocialLogonUser(user, doCommit);

        //                }
        //                else
        //                {
        //                    // Do an update to make sure we have the most recent details
        //                    user.Email = fetch.GetAttributeValue(WellKnownAttributes.Contact.Email);
        //                    user.MiscAccessToken = oid;

        //                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
        //                    {
        //                        Message = LocalizationService.GetResourceString("Members.NowLoggedIn"),
        //                        MessageType = GenericMessages.success
        //                    };

        //                    // Log the user in
        //                    FormsAuthentication.SetAuthCookie(user.UserName, true);
        //                }

        //                if (doCommit)
        //                {
        //                    try
        //                    {
        //                        unitOfWork.Commit();
        //                        SendEmailConfirmationEmail(user);
        //                        return RedirectToAction("Index", "Home");
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        unitOfWork.Rollback();
        //                        LoggingService.Error(ex);
        //                        FormsAuthentication.SignOut();
        //                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
        //                        {
        //                            Message = LocalizationService.GetResourceString("Errors.GenericMessage"),
        //                            MessageType = GenericMessages.error
        //                        };

        //                    }
        //                }

        //            }
        //            break;

        //        case AuthenticationStatus.Canceled:
        //            // Bugger. User cancelled for some reason
        //            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
        //            {
        //                Message = LocalizationService.GetResourceString("Members.LoginCancelledByUser"),
        //                MessageType = GenericMessages.error
        //            };
        //            break;

        //        case AuthenticationStatus.Failed:
        //            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
        //            {
        //                Message = LocalizationService.GetResourceString("Members.LoginFailedByOpenID"),
        //                MessageType = GenericMessages.error
        //            };
        //            break;
        //    }

        //    // Only add this if one hasn't been added already
        //    if (TempData[AppConstants.MessageViewBagName] == null)
        //    {
        //        // Either cancelled or there was an error
        //        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
        //        {
        //            Message = LocalizationService.GetResourceString("Errors.GenericMessage"),
        //            MessageType = GenericMessages.error
        //        };
        //    }
        //    return RedirectToAction("LogOn");
        //}

        //#endregion


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
                var usersDate = LoggedOnUser.LastActivityDate ?? DateTime.Now.AddDays(-1);

                var span = rightNow.Subtract(usersDate);
                var totalMins = span.TotalMinutes;

                if (totalMins > SiteConstants.TimeSpanInMinutesToDoCheck)
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
                return View(new ViewMemberViewModel { User = member, LoggedOnUserId = loggedonId });
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

                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
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
                        // Set the view bag message here
                        SetRegisterViewBagMessage(manuallyAuthoriseMembers, memberEmailAuthorisationNeeded, userToSave);

                        if (!manuallyAuthoriseMembers && !memberEmailAuthorisationNeeded)
                        {
                            homeRedirect = true;
                        }

                        try
                        {
                            unitOfWork.Commit();

                            // Only send the email if the admin is not manually authorising emails or it's pointless
                            SendEmailConfirmationEmail(userToSave);

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

                    return View();
                }
            }
            return RedirectToAction("Index", "Home");
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
                        EmailFrom = SettingsService.GetSettings().NotificationReplyEmail,
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
                        Expires = DateTime.Now.AddDays(7)
                    };
                    // Add the cookie.
                    Response.Cookies.Add(myCookie);
                }
            }
        }

        public ActionResult ResendEmailConfirmation(string username)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
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
                            Expires = DateTime.Now.AddDays(-1)
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
                    var topicViewModels = ViewModelMapping.CreateTopicViewModels(topics, RoleService, UsersRole, LoggedOnUser, _topicNotificationService, _pollAnswerService, _voteService, SettingsService.GetSettings());

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

        [Authorize]
        public ActionResult Edit(Guid id)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {

                var user = MembershipService.GetUser(id);
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

                return View(viewModel);
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult Edit(MemberFrontEndEditViewModel userModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var user = MembershipService.GetUser(userModel.Id);

                // Sort image out first
                if (userModel.Files != null)
                {
                    // Before we save anything, check the user already has an upload folder and if not create one
                    var uploadFolderPath = Server.MapPath(string.Concat(SiteConstants.UploadFolderPath, LoggedOnUser.Id));
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
                user.Facebook = _bannedWordService.SanitiseBannedWords(userModel.Facebook);
                user.Location = _bannedWordService.SanitiseBannedWords(userModel.Location);
                user.Signature = _bannedWordService.SanitiseBannedWords(StringUtils.ScrubHtml(userModel.Signature));
                user.Twitter = _bannedWordService.SanitiseBannedWords(userModel.Twitter);
                user.Website = _bannedWordService.SanitiseBannedWords(userModel.Website);

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
                var sanitisedUsername = _bannedWordService.SanitiseBannedWords(userModel.UserName);
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

                ViewBag.Message = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Member.ProfileUpdated"),
                    MessageType = GenericMessages.success
                };

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
                    Avatar = user.Avatar
                };

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

                return View(viewModel);
            }
        }

        [Authorize]
        public PartialViewResult SideAdminPanel()
        {
            var count = _privateMessageService.NewPrivateMessageCount(LoggedOnUser.Id);
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
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    var user = MembershipService.GetUser(viewModel.Id);
                    var report = new Report
                                     {
                                         Reason = viewModel.Reason,
                                         ReportedMember = user,
                                         Reporter = LoggedOnUser
                                     };
                    _reportService.MemberReport(report);
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
            using (UnitOfWorkManager.NewUnitOfWork())
            {

                if (changePasswordSucceeded)
                {
                    var sb = new StringBuilder();
                    sb.AppendFormat("<p>{0}</p>", string.Format(LocalizationService.GetResourceString("Members.ForgotPassword.Email"), SettingsService.GetSettings().ForumName));
                    sb.AppendFormat("<p><b>{0}</b></p>", newPassword);
                    var email = new Email
                                    {
                                        EmailFrom = SettingsService.GetSettings().NotificationReplyEmail,
                                        EmailTo = currentUser.Email,
                                        NameTo = currentUser.UserName,
                                        Subject = LocalizationService.GetResourceString("Members.ForgotPassword.Subject")
                                    };
                    email.Body = _emailService.EmailTemplate(email.NameTo, sb.ToString());
                    _emailService.SendMail(email);

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
