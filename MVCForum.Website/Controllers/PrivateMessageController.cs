namespace MVCForum.Website.Controllers
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Web.Mvc;
    using Domain.Constants;
    using Domain.DomainModel;
    using Domain.Interfaces.Services;
    using Domain.Interfaces.UnitOfWork;
    using Utilities;
    using Application;
    using Areas.Admin.ViewModels;
    using ViewModels;

    [Authorize]
    public partial class PrivateMessageController : BaseController
    {
        private readonly IPrivateMessageService _privateMessageService;
        private readonly IEmailService _emailService;
        private readonly IConfigService _configService;

        public PrivateMessageController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService,
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService, IPrivateMessageService privateMessageService,
            IEmailService emailService, IConfigService configService, ICacheService cacheService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService, cacheService)
        {
            _privateMessageService = privateMessageService;
            _emailService = emailService;
            _configService = configService;
        }

        public ActionResult Index(int? p)
        {
            if (LoggedOnReadOnlyUser.DisablePrivateMessages == true)
            {
                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Errors.NoPermission"),
                    MessageType = GenericMessages.danger
                };
                return RedirectToAction("Index", "Home");
            }
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var pageIndex = p ?? 1;
                var pagedMessages = _privateMessageService.GetUsersPrivateMessages(pageIndex, SiteConstants.Instance.PrivateMessageListSize, LoggedOnReadOnlyUser);
                var viewModel = new ListPrivateMessageViewModel
                {
                    Messages = pagedMessages,
                    PageIndex = pageIndex,
                    TotalCount = pagedMessages.TotalCount,
                    TotalPages = pagedMessages.TotalPages
                };
                return View(viewModel);
            }
        }

        [ChildActionOnly]
        public ActionResult Create(Guid to)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var viewModel = new CreatePrivateMessageViewModel
                {
                    To = to
                };

                try
                {
                    var permissions = RoleService.GetPermissions(null, UsersRole);
                    var settings = SettingsService.GetSettings();
                    // Check if private messages are enabled
                    if (!settings.EnablePrivateMessages || LoggedOnReadOnlyUser.DisablePrivateMessages == true)
                    {
                        return Content(LocalizationService.GetResourceString("Errors.GenericMessage"));
                    }

                    // Check outbox size of logged in user
                    var senderCount = _privateMessageService.GetAllSentByUser(LoggedOnReadOnlyUser.Id).Count;
                    if (senderCount > settings.MaxPrivateMessagesPerMember)
                    {
                        return Content(LocalizationService.GetResourceString("PM.SentItemsOverCapcity"));
                    }
                    if (senderCount > (settings.MaxPrivateMessagesPerMember - SiteConstants.Instance.PrivateMessageWarningAmountLessThanAllowedSize))
                    {
                        // Send user a warning they are about to exceed 
                        var sb = new StringBuilder();
                        sb.Append($"<p>{LocalizationService.GetResourceString("PM.AboutToExceedInboxSizeBody")}</p>");
                        var email = new Email
                        {
                            EmailTo = LoggedOnReadOnlyUser.Email,
                            NameTo = LoggedOnReadOnlyUser.UserName,
                            Subject = LocalizationService.GetResourceString("PM.AboutToExceedInboxSizeSubject")
                        };
                        email.Body = _emailService.EmailTemplate(email.NameTo, sb.ToString());
                        _emailService.SendMail(email);
                    }

                    // Set editor permissions
                    ViewBag.ImageUploadType = permissions[SiteConstants.Instance.PermissionInsertEditorImages].IsTicked ? "forumimageinsert" : "image";

                    unitOfWork.Commit();

                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                }

                return PartialView(viewModel);
            }

        }

        [HttpPost]
        public ActionResult Create(CreatePrivateMessageViewModel createPrivateMessageViewModel)
        {
            var settings = SettingsService.GetSettings();
            if (!settings.EnablePrivateMessages || LoggedOnReadOnlyUser.DisablePrivateMessages == true)
            {
                throw new Exception(LocalizationService.GetResourceString("Errors.GenericMessage"));
            }
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                if (ModelState.IsValid)
                {
                    var loggedOnUser = MembershipService.GetUser(LoggedOnReadOnlyUser.Id);
                    var memberTo = MembershipService.GetUser(createPrivateMessageViewModel.To);

                    // Check the user they are trying to message hasn't blocked them
                    if (loggedOnUser.BlockedByOtherUsers.Any(x => x.Blocker.Id == memberTo.Id))
                    {
                        return Content(PmAjaxError(LocalizationService.GetResourceString("PM.BlockedMessage")));
                    }

                    // Check flood control
                    var lastMessage = _privateMessageService.GetLastSentPrivateMessage(LoggedOnReadOnlyUser.Id);
                    // If this message they are sending now, is to the same person then ignore flood control
                    if (lastMessage != null && createPrivateMessageViewModel.To != lastMessage.UserTo.Id)
                    {
                        if (DateUtils.TimeDifferenceInSeconds(DateTime.UtcNow, lastMessage.DateSent) < settings.PrivateMessageFloodControl)
                        {
                            return Content(PmAjaxError(LocalizationService.GetResourceString("PM.SendingToQuickly")));
                        }
                    }

                    // first check they are not trying to message themself!
                    if (memberTo != null)
                    {
                        // Map the view model to message
                        var privateMessage = new PrivateMessage
                        {
                            UserFrom = loggedOnUser,
                            Message = createPrivateMessageViewModel.Message,
                        };

                        // Check settings
                        if (settings.EnableEmoticons == true)
                        {
                            privateMessage.Message = _configService.Emotify(privateMessage.Message);
                        }

                        // check the member
                        if (!String.Equals(memberTo.UserName, LoggedOnReadOnlyUser.UserName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            // Check in box size for both
                            var receiverCount = _privateMessageService.GetAllReceivedByUser(memberTo.Id).Count;
                            if (receiverCount > settings.MaxPrivateMessagesPerMember)
                            {
                                return  Content(string.Format(LocalizationService.GetResourceString("PM.ReceivedItemsOverCapcity"), memberTo.UserName));
                            }

                            // If the receiver is about to go over the allowance them let then know too
                            if (receiverCount > (settings.MaxPrivateMessagesPerMember - SiteConstants.Instance.PrivateMessageWarningAmountLessThanAllowedSize))
                            {
                                // Send user a warning they are about to exceed 
                                var sb = new StringBuilder();
                                sb.Append($"<p>{LocalizationService.GetResourceString("PM.AboutToExceedInboxSizeBody")}</p>");
                                var email = new Email
                                {
                                    EmailTo = memberTo.Email,
                                    NameTo = memberTo.UserName,
                                    Subject = LocalizationService.GetResourceString("PM.AboutToExceedInboxSizeSubject")
                                };
                                email.Body = _emailService.EmailTemplate(email.NameTo, sb.ToString());
                                _emailService.SendMail(email);
                            }

                            // Good to go send the message!
                            privateMessage.UserTo = memberTo;
                            _privateMessageService.Add(privateMessage);

                            try
                            {
                                // Finally send an email to the user so they know they have a new private message
                                // As long as they have not had notifications disabled
                                if (memberTo.DisableEmailNotifications != true)
                                {
                                    var email = new Email
                                    {
                                        EmailTo = memberTo.Email,
                                        Subject = LocalizationService.GetResourceString("PM.NewPrivateMessageSubject"),
                                        NameTo = memberTo.UserName
                                    };

                                    var sb = new StringBuilder();
                                    sb.Append($"<p>{string.Format(LocalizationService.GetResourceString("PM.NewPrivateMessageBody"), LoggedOnReadOnlyUser.UserName)}</p>");
                                    sb.Append(AppHelpers.ConvertPostContent(createPrivateMessageViewModel.Message));
                                    email.Body = _emailService.EmailTemplate(email.NameTo, sb.ToString());
                                    _emailService.SendMail(email);
                                }

                                unitOfWork.Commit();

                                return PartialView("_PrivateMessage", privateMessage);
                            }
                            catch (Exception ex)
                            {
                                unitOfWork.Rollback();
                                LoggingService.Error(ex);
                                return Content(PmAjaxError(LocalizationService.GetResourceString("Errors.GenericMessage")));
                            }
                        }
                        else
                        {
                            return Content(PmAjaxError(LocalizationService.GetResourceString("PM.TalkToSelf")));
                        }
                    }
                    else
                    {
                        // Error send back to user
                        return Content(PmAjaxError(LocalizationService.GetResourceString("PM.UnableFindMember")));
                    }
                }
                return Content(PmAjaxError(LocalizationService.GetResourceString("Errors.GenericMessage")));
            }
        }

        public ActionResult View(Guid from)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var userFrom = MembershipService.GetUser(from);
                var loggedOnUser = MembershipService.GetUser(LoggedOnReadOnlyUser.Id);
                if (userFrom.Id != LoggedOnReadOnlyUser.Id)
                {
                    // Mark all messages read sent to this user from the userFrom
                    var unreadMessages = loggedOnUser.PrivateMessagesReceived.Where(x => x.UserFrom.Id == from && !x.IsRead);

                    foreach (var message in unreadMessages)
                    {
                        // Update message as read
                        message.IsRead = true;

                        // Get the sent version and update that too
                        var sentMessage = _privateMessageService.GetMatchingSentPrivateMessage(message.DateSent, message.UserFrom.Id, message.UserTo.Id);
                        if (sentMessage != null)
                        {
                            sentMessage.IsRead = true;
                        }
                    }

                    // Commit all changes
                    try
                    {
                        unitOfWork.Commit();
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        LoggingService.Error(ex);
                    }

                    // Get all the received messages from userFrom
                    // and then get all the sent messages to userFrom
                    // TODO - This is shit, and needs updating
                    //var allMessages = loggedOnUser.PrivateMessagesReceived.Where(x => x.UserFrom.Id == from && x.IsSentMessage == false).ToList();
                    //allMessages.AddRange(loggedOnUser.PrivateMessagesSent.Where(x => x.UserTo.Id == from && x.IsSentMessage == true).ToList());

                    var allMessages = _privateMessageService.GetUsersPrivateMessages(1, SiteConstants.Instance.PagingGroupSize, loggedOnUser, userFrom);

                    // Now order them into an order of messages
                    var date = DateTime.UtcNow.AddMinutes(-AppConstants.TimeSpanInMinutesToShowMembers);

                    var viewModel = new ViewPrivateMessageViewModel
                    {
                        From = userFrom,
                        PrivateMessages = allMessages,
                        FromUserIsOnline = userFrom.LastActivityDate > date,
                        IsAjaxRequest = Request.IsAjaxRequest(),
                        IsBlocked = loggedOnUser.BlockedUsers.Any(x => x.Blocked.Id == userFrom.Id)
                    };

                    return View(viewModel);
                }
                var noPermissionText = LocalizationService.GetResourceString("Errors.NoPermission");
                if (Request.IsAjaxRequest())
                {
                    return Content(noPermissionText);
                }
                return ErrorToHomePage(noPermissionText);
            }
        }

        [HttpPost]
        public ActionResult Delete(DeletePrivateMessageViewModel deletePrivateMessageViewModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                if (Request.IsAjaxRequest())
                {
                    var privateMessage = _privateMessageService.Get(deletePrivateMessageViewModel.Id);
                    if (privateMessage.UserTo.Id == LoggedOnReadOnlyUser.Id | privateMessage.UserFrom.Id == LoggedOnReadOnlyUser.Id)
                    {
                        _privateMessageService.DeleteMessage(privateMessage);
                    }
                    else
                    {
                        throw new Exception(LocalizationService.GetResourceString("Errors.NoPermission"));
                    }
                }

                try
                {
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    throw new Exception(LocalizationService.GetResourceString("Errors.GenericMessage"));
                }
            }

            return null;
        }

        [HttpPost]
        public ActionResult AjaxMore(GetMoreViewModel viewModel)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                if (Request.IsAjaxRequest())
                {
                    var userFrom = MembershipService.GetUser(viewModel.UserId);
                    var loggedOnUser = MembershipService.GetUser(LoggedOnReadOnlyUser.Id);

                    var settings = SettingsService.GetSettings();
                    if (!settings.EnablePrivateMessages || LoggedOnReadOnlyUser.DisablePrivateMessages == true)
                    {
                        return Content(LocalizationService.GetResourceString("Errors.GenericMessage"));
                    }

                    var allMessages = _privateMessageService.GetUsersPrivateMessages(viewModel.PageIndex, SiteConstants.Instance.PagingGroupSize, loggedOnUser, userFrom);

                    var partialViewModel = new ViewPrivateMessageViewModel
                    {
                        From = userFrom,
                        PrivateMessages = allMessages,
                        IsAjaxRequest = Request.IsAjaxRequest(),
                    };

                    return PartialView(partialViewModel);
                }
                return Content(string.Empty);
            }
        }

        private static string PmAjaxError(string message)
        {
            return $"<p class=\"pmerrormessage\">{message}</p>";
        }

        internal ActionResult ErrorToInbox(string errorMessage)
        {
            // Use temp data as its a redirect
            TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = errorMessage,
                MessageType = GenericMessages.danger
            };
            // Not allowed in here so
            return RedirectToAction("Index", "PrivateMessage");
        }
    }
}
