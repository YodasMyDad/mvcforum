namespace MvcForum.Web.Areas.Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using Core.Constants;
    using Core.DomainModel.Entities;
    using Core.Interfaces.Services;
    using Core.Interfaces.UnitOfWork;
    using ViewModels;

    [Authorize(Roles = AppConstants.AdminRoleName)]
    public class BatchController : BaseAdminController
    {
        private readonly ICategoryService _categoryService;
        private readonly IPrivateMessageService _privateMessageService;
        private readonly ITopicService _topicService;

        public BatchController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager,
            IMembershipService membershipService,
            ILocalizationService localizationService, ISettingsService settingsService,
            ICategoryService categoryService, ITopicService topicService, IPrivateMessageService privateMessageService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, settingsService)
        {
            _categoryService = categoryService;
            _topicService = topicService;
            _privateMessageService = privateMessageService;
        }

        #region Members

        public ActionResult BatchDeleteMembers()
        {
            return View(new BatchDeleteMembersViewModel {AmoutOfDaysSinceRegistered = 0, AmoutOfPosts = 0});
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BatchDeleteMembers(BatchDeleteMembersViewModel viewModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var membersToDelete = MembershipService.GetUsersByDaysPostsPoints(
                        viewModel.AmoutOfDaysSinceRegistered,
                        viewModel.AmoutOfPosts);
                    var count = membersToDelete.Count;
                    foreach (var membershipUser in membersToDelete)
                    {
                        MembershipService.Delete(membershipUser, unitOfWork);
                    }
                    unitOfWork.Commit();
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = string.Format("{0} members deleted", count),
                        MessageType = GenericMessages.success
                    };
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = ex.Message,
                        MessageType = GenericMessages.danger
                    };
                }
            }

            return View();
        }

        #endregion

        #region Topics

        public ActionResult BatchMoveTopics()
        {
            var viewModel = new BatchMoveTopicsViewModel
            {
                Categories = _categoryService.GetAll()
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BatchMoveTopics(BatchMoveTopicsViewModel viewModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var categoryFrom = _categoryService.Get((Guid) viewModel.FromCategory);
                    var categoryTo = _categoryService.Get((Guid) viewModel.ToCategory);

                    var topicsToMove = _topicService.GetRssTopicsByCategory(int.MaxValue, categoryFrom.Id);
                    var count = topicsToMove.Count;

                    foreach (var topic in topicsToMove)
                    {
                        topic.Category = categoryTo;
                    }

                    unitOfWork.SaveChanges();

                    categoryFrom.Topics.Clear();

                    viewModel.Categories = _categoryService.GetAll();

                    unitOfWork.Commit();

                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = string.Format("{0} topics moved", count),
                        MessageType = GenericMessages.success
                    };
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = ex.Message,
                        MessageType = GenericMessages.danger
                    };
                }
            }

            return View(viewModel);
        }

        #endregion

        #region Private Messages

        public ActionResult BatchDeletePrivateMessages()
        {
            var viewModel = new BatchDeletePrivateMessagesViewModel();
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult BatchDeletePrivateMessages(BatchDeletePrivateMessagesViewModel viewModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var pms = _privateMessageService.GetPrivateMessagesOlderThan(viewModel.Days);
                    var pmToDelete = new List<PrivateMessage>();
                    pmToDelete.AddRange(pms);
                    var count = pmToDelete.Count;
                    foreach (var pm in pmToDelete)
                    {
                        _privateMessageService.DeleteMessage(pm);
                    }
                    unitOfWork.Commit();

                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = string.Format("{0} Private Messages deleted", count),
                        MessageType = GenericMessages.success
                    };
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = ex.Message,
                        MessageType = GenericMessages.danger
                    };
                }
            }

            return View(viewModel);
        }

        #endregion
    }
}