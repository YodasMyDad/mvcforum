namespace MvcForum.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Core;
    using Core.Constants;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Core.Models;
    using Core.Models.Entities;
    using Core.Utilities;
    using ViewModels;
    using ViewModels.PrivateMessage;

    [Authorize]
    public partial class PrivateMessageController : BaseController
    {
        private readonly IConfigService _configService;
        private readonly IEmailService _emailService;
        private readonly IPrivateMessageService _privateMessageService;

        public PrivateMessageController(ILoggingService loggingService, IMembershipService membershipService,
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService,
            IPrivateMessageService privateMessageService, IEmailService emailService, IConfigService configService,
            ICacheService cacheService, IMvcForumContext context)
            : base(loggingService, membershipService, localizationService, roleService,
                settingsService, cacheService, context)
        {
            _privateMessageService = privateMessageService;
            _emailService = emailService;
            _configService = configService;
        }

        public virtual async Task<ActionResult> Index(int? p)
        {
            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

            if (loggedOnReadOnlyUser.DisablePrivateMessages == true)
            {
                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Errors.NoPermission"),
                    MessageType = GenericMessages.danger
                };
                return RedirectToAction("Index", "Home");
            }

            var pageIndex = p ?? 1;
            var pagedMessages = await _privateMessageService.GetUsersPrivateMessages(pageIndex,
                ForumConfiguration.Instance.PrivateMessageListSize, loggedOnReadOnlyUser);
            var viewModel = new ListPrivateMessageViewModel
            {
                Messages = pagedMessages,
                PageIndex = pageIndex,
                TotalCount = pagedMessages.TotalCount,
                TotalPages = pagedMessages.TotalPages
            };
            return View(viewModel);
        }

        [ChildActionOnly]
        public virtual ActionResult Create(Guid to)
        {
            var viewModel = new CreatePrivateMessageViewModel
            {
                To = to
            };

            try
            {
                var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
                var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

                var permissions = RoleService.GetPermissions(null, loggedOnUsersRole);
                var settings = SettingsService.GetSettings();
                // Check if private messages are enabled
                if (!settings.EnablePrivateMessages || loggedOnReadOnlyUser.DisablePrivateMessages == true)
                {
                    return Content(LocalizationService.GetResourceString("Errors.GenericMessage"));
                }

                // Check outbox size of logged in user
                var senderCount = _privateMessageService.GetAllSentByUser(loggedOnReadOnlyUser.Id).Count;
                if (senderCount > settings.MaxPrivateMessagesPerMember)
                {
                    return Content(LocalizationService.GetResourceString("PM.SentItemsOverCapcity"));
                }
                if (senderCount > settings.MaxPrivateMessagesPerMember -
                    ForumConfiguration.Instance.PrivateMessageWarningAmountLessThanAllowedSize)
                {
                    // Send user a warning they are about to exceed 
                    var sb = new StringBuilder();
                    sb.Append($"<p>{LocalizationService.GetResourceString("PM.AboutToExceedInboxSizeBody")}</p>");
                    var email = new Email
                    {
                        EmailTo = loggedOnReadOnlyUser.Email,
                        NameTo = loggedOnReadOnlyUser.UserName,
                        Subject = LocalizationService.GetResourceString("PM.AboutToExceedInboxSizeSubject")
                    };
                    email.Body = _emailService.EmailTemplate(email.NameTo, sb.ToString());
                    _emailService.SendMail(email);
                }

                // Set editor permissions
                ViewBag.ImageUploadType = permissions[ForumConfiguration.Instance.PermissionInsertEditorImages].IsTicked
                    ? "forumimageinsert"
                    : "image";

                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
            }

            return PartialView(viewModel);
        }

        [HttpPost]
        public virtual ActionResult Create(CreatePrivateMessageViewModel createPrivateMessageViewModel)
        {
            if (ModelState.IsValid)
            {
                var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
                var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

                var settings = SettingsService.GetSettings();
                if (!settings.EnablePrivateMessages || loggedOnReadOnlyUser.DisablePrivateMessages == true)
                {
                    throw new Exception(LocalizationService.GetResourceString("Errors.GenericMessage"));
                }

                var loggedOnUser = MembershipService.GetUser(loggedOnReadOnlyUser.Id);
                var memberTo = MembershipService.GetUser(createPrivateMessageViewModel.To);

                // Check the user they are trying to message hasn't blocked them
                if (loggedOnUser.BlockedByOtherUsers.Any(x => x.Blocker.Id == memberTo.Id))
                {
                    return Content(PmAjaxError(LocalizationService.GetResourceString("PM.BlockedMessage")));
                }

                // Check flood control
                var lastMessage = _privateMessageService.GetLastSentPrivateMessage(loggedOnReadOnlyUser.Id);
                // If this message they are sending now, is to the same person then ignore flood control
                if (lastMessage != null && createPrivateMessageViewModel.To != lastMessage.UserTo.Id)
                {
                    if (DateUtils.TimeDifferenceInSeconds(DateTime.UtcNow, lastMessage.DateSent) <
                        settings.PrivateMessageFloodControl)
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
                        Message = createPrivateMessageViewModel.Message
                    };

                    // Check settings
                    if (settings.EnableEmoticons == true)
                    {
                        privateMessage.Message = _configService.Emotify(privateMessage.Message);
                    }

                    // check the member
                    if (!string.Equals(memberTo.UserName, loggedOnReadOnlyUser.UserName,
                        StringComparison.CurrentCultureIgnoreCase))
                    {
                        // Check in box size for both
                        var receiverCount = _privateMessageService.GetAllReceivedByUser(memberTo.Id).Count;
                        if (receiverCount > settings.MaxPrivateMessagesPerMember)
                        {
                            return Content(string.Format(
                                LocalizationService.GetResourceString("PM.ReceivedItemsOverCapcity"),
                                memberTo.UserName));
                        }

                        // If the receiver is about to go over the allowance them let then know too
                        if (receiverCount > settings.MaxPrivateMessagesPerMember -
                            ForumConfiguration.Instance.PrivateMessageWarningAmountLessThanAllowedSize)
                        {
                            // Send user a warning they are about to exceed 
                            var sb = new StringBuilder();
                            sb.Append(
                                $"<p>{LocalizationService.GetResourceString("PM.AboutToExceedInboxSizeBody")}</p>");
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
                                sb.Append(
                                    $"<p>{string.Format(LocalizationService.GetResourceString("PM.NewPrivateMessageBody"), loggedOnReadOnlyUser.UserName)}</p>");
                                sb.Append(createPrivateMessageViewModel.Message.ConvertPostContent());
                                email.Body = _emailService.EmailTemplate(email.NameTo, sb.ToString());
                                _emailService.SendMail(email);
                            }

                            Context.SaveChanges();

                            return PartialView("_PrivateMessage", privateMessage);
                        }
                        catch (Exception ex)
                        {
                            Context.RollBack();
                            LoggingService.Error(ex);
                            return Content(
                                PmAjaxError(LocalizationService.GetResourceString("Errors.GenericMessage")));
                        }
                    }
                    return Content(PmAjaxError(LocalizationService.GetResourceString("PM.TalkToSelf")));
                }
                // Error send back to user
                return Content(PmAjaxError(LocalizationService.GetResourceString("PM.UnableFindMember")));
            }
            return Content(PmAjaxError(LocalizationService.GetResourceString("Errors.GenericMessage")));
        }

        public virtual async Task<ActionResult> View(Guid from)
        {
            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);

            var userFrom = MembershipService.GetUser(from);
            var loggedOnUser = MembershipService.GetUser(loggedOnReadOnlyUser.Id);
            if (userFrom.Id != loggedOnReadOnlyUser.Id)
            {
                // Mark all messages read sent to this user from the userFrom
                var unreadMessages =
                    loggedOnUser.PrivateMessagesReceived.Where(x => x.UserFrom.Id == from && !x.IsRead);

                foreach (var message in unreadMessages)
                {
                    // Update message as read
                    message.IsRead = true;

                    // Get the sent version and update that too
                    var sentMessage = _privateMessageService.GetMatchingSentPrivateMessage(message.DateSent,
                        message.UserFrom.Id, message.UserTo.Id);
                    if (sentMessage != null)
                    {
                        sentMessage.IsRead = true;
                    }
                }

                // Commit all changes
                try
                {
                    Context.SaveChanges();
                }
                catch (Exception ex)
                {
                    Context.RollBack();
                    LoggingService.Error(ex);
                }

                // Get all the received messages from userFrom
                // and then get all the sent messages to userFrom
                // TODO - This is shit, and needs updating
                //var allMessages = loggedOnUser.PrivateMessagesReceived.Where(x => x.UserFrom.Id == from && x.IsSentMessage == false).ToList();
                //allMessages.AddRange(loggedOnUser.PrivateMessagesSent.Where(x => x.UserTo.Id == from && x.IsSentMessage == true).ToList());

                var allMessages = await _privateMessageService.GetUsersPrivateMessages(1,
                    ForumConfiguration.Instance.PagingGroupSize, loggedOnUser, userFrom);

                // Now order them into an order of messages
                var date = DateTime.UtcNow.AddMinutes(-Constants.TimeSpanInMinutesToShowMembers);

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

        [HttpPost]
        public virtual ActionResult Delete(DeletePrivateMessageViewModel deletePrivateMessageViewModel)
        {
            if (Request.IsAjaxRequest())
            {
                var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
                var privateMessage = _privateMessageService.Get(deletePrivateMessageViewModel.Id);
                if ((privateMessage.UserTo.Id == loggedOnReadOnlyUser.Id) |
                    (privateMessage.UserFrom.Id == loggedOnReadOnlyUser.Id))
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
                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
                throw new Exception(LocalizationService.GetResourceString("Errors.GenericMessage"));
            }


            return null;
        }

        [HttpPost]
        public virtual async Task<ActionResult> AjaxMore(GetMoreViewModel viewModel)
        {
            if (Request.IsAjaxRequest())
            {
                var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
                var userFrom = MembershipService.GetUser(viewModel.UserId);
                var loggedOnUser = MembershipService.GetUser(loggedOnReadOnlyUser.Id);

                var settings = SettingsService.GetSettings();
                if (!settings.EnablePrivateMessages || loggedOnReadOnlyUser.DisablePrivateMessages == true)
                {
                    return Content(LocalizationService.GetResourceString("Errors.GenericMessage"));
                }

                var allMessages = await _privateMessageService.GetUsersPrivateMessages(viewModel.PageIndex,
                    ForumConfiguration.Instance.PagingGroupSize, loggedOnUser, userFrom);

                var partialViewModel = new ViewPrivateMessageViewModel
                {
                    From = userFrom,
                    PrivateMessages = allMessages,
                    IsAjaxRequest = Request.IsAjaxRequest()
                };

                return PartialView(partialViewModel);
            }
            return Content(string.Empty);
        }

        private static string PmAjaxError(string message)
        {
            return $"<p class=\"pmerrormessage\">{message}</p>";
        }

        internal virtual ActionResult ErrorToInbox(string errorMessage)
        {
            // Use temp data as its a redirect
            TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = errorMessage,
                MessageType = GenericMessages.danger
            };
            // Not allowed in here so
            return RedirectToAction("Index", "PrivateMessage");
        }
    }
}