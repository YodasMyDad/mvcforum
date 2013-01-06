using System;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Utilities;
using MVCForum.Website.Areas.Admin.ViewModels;
using MVCForum.Website.ViewModels;

namespace MVCForum.Website.Controllers
{
    [Authorize]
    public class PrivateMessageController : BaseController
    {
        private readonly IPrivateMessageService _privateMessageService;

        public PrivateMessageController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService,
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService, IPrivateMessageService privateMessageService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _privateMessageService = privateMessageService;
        }

        public ActionResult Index(int? p)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var pageIndex = p ?? 1;
                var pagedMessages = _privateMessageService.GetPagedReceivedMessagesByUser(pageIndex, AppConstants.PrivateMessageListSize, LoggedOnUser);
                var viewModel = new ListPrivateMessageViewModel
                {
                    Messages = pagedMessages,
                    PageIndex = pageIndex,
                    TotalCount = pagedMessages.TotalCount
                };
                return View(viewModel);
            }
        }

        public ActionResult SentMessages(int? p)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var pageIndex = p ?? 1;
                var pagedMessages = _privateMessageService.GetPagedSentMessagesByUser(pageIndex, AppConstants.PrivateMessageListSize, LoggedOnUser);
                var viewModel = new ListPrivateMessageViewModel
                {
                    Messages = pagedMessages
                };
                return View(viewModel);
            }
        }

        public ActionResult Create(Guid? id)
        {
            // Check if private messages are enabled
            if(!SettingsService.GetSettings().EnablePrivateMessages)
            {
                return ErrorToHomePage(LocalizationService.GetResourceString("Errors.GenericMessage"));
            }

            // Check flood control
            var lastMessage = _privateMessageService.GetLastSentPrivateMessage(LoggedOnUser.Id);
            if (lastMessage != null && DateUtils.TimeDifferenceInMinutes(DateTime.Now, lastMessage.DateSent) < SettingsService.GetSettings().PrivateMessageFloodControl)
            {
                return ErrorToInbox(LocalizationService.GetResourceString("PM.SendingToQuickly"));
            }

            // Check outbox size
            var senderCount = _privateMessageService.GetAllSentByUser(LoggedOnUser.Id).Count;
            if (senderCount > SettingsService.GetSettings().MaxPrivateMessagesPerMember)
            {
                return ErrorToInbox(LocalizationService.GetResourceString("PM.SentItemsOverCapcity"));
            }

            var viewModel = new CreatePrivateMessageViewModel();
            // See if this is a reply or not
            if (id != null)
            {
                var previousMessage = _privateMessageService.Get((Guid)id);
                // Its a reply, get the details
                viewModel.UserToUsername = previousMessage.UserFrom.UserName;
                viewModel.Subject = previousMessage.Subject;
                viewModel.PreviousMessage = previousMessage.Message;
            }
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(CreatePrivateMessageViewModel createPrivateMessageViewModel)
        {
            if (!SettingsService.GetSettings().EnablePrivateMessages)
            {
                return ErrorToHomePage(LocalizationService.GetResourceString("Errors.GenericMessage"));
            }
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                if (ModelState.IsValid)
                {
                    var userTo = createPrivateMessageViewModel.UserToUsername;

                    // first check they are not trying to message themself!
                    if (userTo.ToLower() != LoggedOnUser.UserName.ToLower())
                    {
                        // Map the view model to message
                        var privateMessage = new PrivateMessage
                                                 {
                                                     UserFrom = LoggedOnUser,
                                                     Subject = createPrivateMessageViewModel.Subject,
                                                     Message = createPrivateMessageViewModel.Message,
                                                 };                            
                        // now get the user its being sent to
                        var memberTo = MembershipService.GetUser(userTo);

                        // check the member
                        if (memberTo != null)
                        {

                            // Check in box size
                            // First check sender
                            var receiverCount = _privateMessageService.GetAllReceivedByUser(memberTo.Id).Count;
                            if (receiverCount > SettingsService.GetSettings().MaxPrivateMessagesPerMember)
                            {
                                ModelState.AddModelError(string.Empty, string.Format(LocalizationService.GetResourceString("PM.ReceivedItemsOverCapcity"), memberTo.UserName));
                            }
                            else
                            {
                                // Good to go send the message!
                                privateMessage.UserTo = memberTo;
                                _privateMessageService.Add(privateMessage);

                                try
                                {
                                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                                    {
                                        Message = LocalizationService.GetResourceString("PM.MessageSent"),
                                        MessageType = GenericMessages.success
                                    };

                                    unitOfWork.Commit();

                                    return RedirectToAction("Index");
                                }
                                catch (Exception ex)
                                {
                                    unitOfWork.Rollback();
                                    LoggingService.Error(ex);
                                    ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("Errors.GenericMessage"));
                                }   
                            }
                        }
                        else
                        {
                            // Error send back to user
                            ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("PM.UnableFindMember"));
                        }
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, LocalizationService.GetResourceString("PM.TalkToSelf"));
                    }
                }
                TempData[AppConstants.MessageViewBagName] = null;
                return View(createPrivateMessageViewModel);
            }
        }

        public ActionResult View(Guid id)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                var message = _privateMessageService.Get(id);

                if (message.UserTo == LoggedOnUser | message.UserFrom == LoggedOnUser)
                {
                    //Mark as read if this is the receiver of the message
                    if (message.UserTo == LoggedOnUser)
                    {
                        // Update message as read
                        message.IsRead = true;

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

                    return View(new ViewPrivateMessageViewModel { Message = message });
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
                MessageType = GenericMessages.error
            };
            // Not allowed in here so
            return RedirectToAction("Index", "PrivateMessage");
        }
    }
}
