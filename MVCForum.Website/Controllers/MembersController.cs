using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using System.Web.Security;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Utilities;
using MVCForum.Website.Areas.Admin.ViewModels;
using MVCForum.Website.ViewModels;
using MembershipCreateStatus = MVCForum.Domain.DomainModel.MembershipCreateStatus;
using MembershipUser = MVCForum.Domain.DomainModel.MembershipUser;

namespace MVCForum.Website.Controllers
{
    public class MembersController : BaseController
    {
        private readonly IPostService _postService;
        private readonly IReportService _reportService;
        private readonly IEmailService _emailService;
        private readonly IPrivateMessageService _privateMessageService;

        public MembersController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, ILocalizationService localizationService,
            IRoleService roleService, ISettingsService settingsService, IPostService postService, IReportService reportService, IEmailService emailService, IPrivateMessageService privateMessageService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _postService = postService;
            _reportService = reportService;
            _emailService = emailService;
            _privateMessageService = privateMessageService;
        }

        public ActionResult GetByName(string slug)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var member = MembershipService.GetUserBySlug(slug);
                var loggedonId = LoggedOnUser != null ? LoggedOnUser.Id : Guid.Empty;
                return View(new ViewMemberViewModel { User = member, LoggedOnUserId = loggedonId });
            }
        }

        /// <summary>
        /// Add a new user
        /// </summary>
        /// <returns></returns>
        public ActionResult Register()
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var user = MembershipService.CreateEmptyUser();
                var viewModel = new MemberAddViewModel
                                    {
                                        UserName = user.UserName,
                                        Email = user.Email,
                                        Password = user.Password,
                                        IsApproved = user.IsApproved,
                                        Comment = user.Comment,
                                        AllRoles = RoleService.AllRoles()
                                    };

                return View(viewModel);
            }
        }

        //
        // POST /Account/Add
        [HttpPost]
        public ActionResult Register(MemberAddViewModel userModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {                
                var userToSave = new MembershipUser
                {
                    UserName = StringUtils.GetSafeHtml(userModel.UserName),
                    Email = StringUtils.GetSafeHtml(userModel.Email),
                    Password = StringUtils.GetSafeHtml(userModel.Password),
                    IsApproved = userModel.IsApproved,
                    Comment = StringUtils.GetSafeHtml(userModel.Comment),                    
                };

                var homeRedirect = false;

                // Now check settings, see if users need to be manually authorised
                var manuallyAuthoriseMembers = SettingsService.GetSettings().ManuallyAuthoriseNewMembers;
                if (manuallyAuthoriseMembers)
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
                    if (!manuallyAuthoriseMembers)
                    {
                        // If not manually authorise then log the user in
                        FormsAuthentication.SetAuthCookie(userToSave.UserName, false);
                        TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                        {
                            Message = LocalizationService.GetResourceString("Members.NowRegistered"),
                            MessageType = GenericMessages.success
                        };
                        homeRedirect = true;

                    }
                    else
                    {
                        ViewBag.Message = new GenericMessageViewModel
                        {
                            Message = LocalizationService.GetResourceString("Members.NowRegisteredNeedApproval"),
                            MessageType = GenericMessages.success
                        };   
                    }


                    try
                    {
                        unitOfWork.Commit();
                        if(homeRedirect)
                        {
                            return RedirectToAction("Index", "Home", new { area = string.Empty });   
                        }
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                        ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
                    }
                }
                return View();
            }
        }


        /// <summary>
        /// Log on
        /// </summary>
        /// <returns></returns>
        public ActionResult LogOn()
        {
            return View();
        }

        /// <summary>
        /// Log on post
        /// </summary>
        /// <param name="model"></param>
        /// <param name="returnUrl"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult LogOn(LogOnViewModel model, string returnUrl)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var username = StringUtils.SafePlainText(model.UserName);
                var password = StringUtils.SafePlainText(model.Password);

                try
                {
                    if (ModelState.IsValid)
                    {
                        if (MembershipService.ValidateUser(username, password, System.Web.Security.Membership.MaxInvalidPasswordAttempts))
                        {
                            // Set last login date
                            var user = MembershipService.GetUser(username);
                            if (user.IsApproved && !user.IsLockedOut)
                            {
                                FormsAuthentication.SetAuthCookie(username, model.RememberMe);
                                user.LastLoginDate = DateTime.Now;
 
                                // We use temp data because we are doing a redirect
                                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                                {
                                    Message = LocalizationService.GetResourceString("Members.NowLoggedIn"),
                                    MessageType = GenericMessages.success
                                };

                                if (Url.IsLocalUrl(returnUrl) && returnUrl.Length > 1 && returnUrl.StartsWith("/")
                                    && !returnUrl.StartsWith("//") && !returnUrl.StartsWith("/\\"))
                                {
                                    return Redirect(returnUrl);
                                }
                                return RedirectToAction("Index", "Home", new { area = string.Empty });
                            }
                        }

                        // Login failed
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
                                break;

                            default:
                                ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Members.Errors.LogonGeneric"));
                                break;
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
                    var topics = posts.Select(x => x.Topic).Distinct().Take(6).ToList();

                    // Get all the categories for this topic collection
                    var categories = topics.Select(x => x.Category).Distinct();

                    // create the view model
                    var viewModel = new ViewMemberDiscussionsViewModel
                    {
                        Topics = topics,
                        AllPermissionSets = new Dictionary<Category, PermissionSet>(),
                        CurrentUser = LoggedOnUser
                    };

                    // loop through the categories and get the permissions
                    foreach (var category in categories)
                    {
                        var permissionSet = RoleService.GetPermissions(category, UsersRole);
                        viewModel.AllPermissionSets.Add(category, permissionSet);
                    }

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

                user.Age = userModel.Age;
                user.Email = userModel.Email;
                user.Facebook = userModel.Facebook;
                user.Location = userModel.Location;
                user.Signature = userModel.Signature;
                user.Twitter = userModel.Twitter;
                user.UserName = userModel.UserName;
                user.Website = userModel.Website;
                                
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
                };

                try
                {
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                }

                return View(viewModel);
            }
        }

        [Authorize]
        public PartialViewResult SideAdminPanel()
        {
            var count = _privateMessageService.NewPrivateMessageCount(LoggedOnUser.Id);
            return PartialView(new ViewAdminSidePanelViewModel { CurrentUser = LoggedOnUser, NewPrivateMessageCount = count});
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
                                         Reason = StringUtils.SafePlainText(viewModel.Reason),
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
                var allUsers = string.IsNullOrEmpty(search) ? MembershipService.GetAll(pageIndex, AppConstants.AdminListPageSize) :
                                    MembershipService.SearchMembers(search, pageIndex, AppConstants.AdminListPageSize);

                // Redisplay list of users
                var allViewModelUsers = allUsers.Select(user => new PublicSingleMemberListViewModel
                                                                    {
                                                                        UserName = user.UserName, NiceUrl = user.NiceUrl, CreateDate = user.CreateDate, TotalPoints = user.TotalPoints,
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
                    var currentUser = MembershipService.GetUser(User.Identity.Name);
                    changePasswordSucceeded = MembershipService.ChangePassword(currentUser, model.OldPassword, model.NewPassword);

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
            }

            // Success send newpassword to the user telling them password has been changed
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                        var sb = new StringBuilder();
                        sb.AppendFormat("<p>{0}</p>", string.Format(LocalizationService.GetResourceString("Members.ForgotPassword.Email"), SettingsService.GetSettings().ForumName));
                        sb.AppendFormat("<p><b>{0}</b></p>", newPassword);
                        var email = new Email
                                        {
                                            EmailFrom = SettingsService.GetSettings().NotificationReplyEmail,
                                            EmailTo = currentUser.Email,
                                            Body = sb.ToString(),
                                            NameTo = currentUser.UserName,
                                            Subject = LocalizationService.GetResourceString("Members.ForgotPassword.Subject")
                                        };
                        _emailService.SendMail(email);

                        if (changePasswordSucceeded)
                        {
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
