using System;
using System.Linq;
using System.Text;
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
    [Authorize]
    public partial class PrivateMessageController : BaseController
    {
        private readonly IPrivateMessageService _privateMessageService;
        private readonly IEmailService _emailService;

        private MembershipUser LoggedOnUser;

        public PrivateMessageController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService,
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService, IPrivateMessageService privateMessageService,
            IEmailService emailService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _privateMessageService = privateMessageService;
            _emailService = emailService;

            LoggedOnUser = UserIsAuthenticated ? MembershipService.GetUser(Username) : null;
        }

        public ActionResult Index(int? p)
        {
            if (LoggedOnUser.DisablePrivateMessages == true)
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
                var pagedMessages = _privateMessageService.GetPagedReceivedMessagesByUser(pageIndex, SiteConstants.PrivateMessageListSize, LoggedOnUser);
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

                    // Check if private messages are enabled
                    if (!SettingsService.GetSettings().EnablePrivateMessages || LoggedOnUser.DisablePrivateMessages == true)
                    {
                        return Content(LocalizationService.GetResourceString("Errors.GenericMessage"));
                    }

                    // Check outbox size of logged in user
                    var senderCount = _privateMessageService.GetAllSentByUser(LoggedOnUser.Id).Count;
                    if (senderCount > SettingsService.GetSettings().MaxPrivateMessagesPerMember)
                    {
                        return Content(LocalizationService.GetResourceString("PM.SentItemsOverCapcity"));
                    }
                    if (senderCount > (SettingsService.GetSettings().MaxPrivateMessagesPerMember - SiteConstants.PrivateMessageWarningAmountLessThanAllowedSize))
                    {
                        // Send user a warning they are about to exceed 
                        var sb = new StringBuilder();
                        sb.AppendFormat("<p>{0}</p>", LocalizationService.GetResourceString("PM.AboutToExceedInboxSizeBody"));
                        var email = new Email
                        {
                            EmailTo = LoggedOnUser.Email,
                            NameTo = LoggedOnUser.UserName,
                            Subject = LocalizationService.GetResourceString("PM.AboutToExceedInboxSizeSubject")
                        };
                        email.Body = _emailService.EmailTemplate(email.NameTo, sb.ToString());
                        _emailService.SendMail(email);
                    }

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
            if (!SettingsService.GetSettings().EnablePrivateMessages || LoggedOnUser.DisablePrivateMessages == true)
            {
                throw new Exception(LocalizationService.GetResourceString("Errors.GenericMessage"));
            }
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                if (ModelState.IsValid)
                {
                    // Check flood control
                    var lastMessage = _privateMessageService.GetLastSentPrivateMessage(LoggedOnUser.Id);
                    if (lastMessage != null && DateUtils.TimeDifferenceInMinutes(DateTime.UtcNow, lastMessage.DateSent) < SettingsService.GetSettings().PrivateMessageFloodControl)
                    {
                        throw new Exception(LocalizationService.GetResourceString("PM.SendingToQuickly"));
                    }

                    var memberTo = MembershipService.GetUser(createPrivateMessageViewModel.To);

                    // first check they are not trying to message themself!
                    if (memberTo != null)
                    {
                        // Map the view model to message
                        var privateMessage = new PrivateMessage
                        {
                            UserFrom = LoggedOnUser,
                            Message = createPrivateMessageViewModel.Message,
                        };

                        // check the member
                        if (!String.Equals(memberTo.UserName, LoggedOnUser.UserName, StringComparison.CurrentCultureIgnoreCase))
                        {
                            // Check in box size for both
                            var receiverCount = _privateMessageService.GetAllReceivedByUser(memberTo.Id).Count;
                            if (receiverCount > SettingsService.GetSettings().MaxPrivateMessagesPerMember)
                            {
                                throw new Exception(string.Format(LocalizationService.GetResourceString("PM.ReceivedItemsOverCapcity"), memberTo.UserName));
                            }

                            // If the receiver is about to go over the allowance them let then know too
                            if (receiverCount > (SettingsService.GetSettings().MaxPrivateMessagesPerMember - SiteConstants.PrivateMessageWarningAmountLessThanAllowedSize))
                            {
                                // Send user a warning they are about to exceed 
                                var sb = new StringBuilder();
                                sb.AppendFormat("<p>{0}</p>", LocalizationService.GetResourceString("PM.AboutToExceedInboxSizeBody"));
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
                                    sb.AppendFormat("<p>{0}</p>", string.Format(LocalizationService.GetResourceString("PM.NewPrivateMessageBody"), LoggedOnUser.UserName));
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
                                throw new Exception(LocalizationService.GetResourceString("Errors.GenericMessage"));
                            }
                        }
                        else
                        {
                            throw new Exception(LocalizationService.GetResourceString("PM.TalkToSelf"));
                        }
                    }
                    else
                    {
                        // Error send back to user
                        throw new Exception(LocalizationService.GetResourceString("PM.UnableFindMember"));
                    }
                }
                throw new Exception(LocalizationService.GetResourceString("Errors.GenericMessage"));
            }
        }

        public ActionResult View(Guid from)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var userFrom = MembershipService.GetUser(from);

                if (userFrom != LoggedOnUser)
                {
                    // Mark all messages read sent to this user from the userFrom
                    var unreadMessages = LoggedOnUser.PrivateMessagesReceived.Where(x => x.UserFrom.Id == from && !x.IsRead);

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

                    var allMessages = LoggedOnUser.PrivateMessagesReceived.Where(x => x.UserFrom.Id == from && x.IsSentMessage == false).ToList();
                    allMessages.AddRange(LoggedOnUser.PrivateMessagesSent.Where(x => x.UserTo.Id == from && x.IsSentMessage == true).ToList());

                    // Now order them into an order of messages

                    var viewModel = new ViewPrivateMessageViewModel
                    {
                        From = userFrom,
                        PrivateMessages = allMessages.OrderByDescending(x => x.DateSent).ToList()
                    };

                    return View(viewModel);
                }

                return ErrorToHomePage(LocalizationService.GetResourceString("Errors.NoPermission"));
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
                    if (privateMessage.UserTo == LoggedOnUser | privateMessage.UserFrom == LoggedOnUser)
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
