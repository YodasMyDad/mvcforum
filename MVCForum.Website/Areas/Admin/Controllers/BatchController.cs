namespace MvcForum.Web.Areas.Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using Core.Constants;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Core.Models.Entities;
    using ViewModels;

    [Authorize(Roles = Constants.AdminRoleName)]
    public class BatchController : BaseAdminController
    {
        private readonly ICategoryService _categoryService;
        private readonly IPrivateMessageService _privateMessageService;
        private readonly ITopicService _topicService;

        public BatchController(ILoggingService loggingService, IMembershipService membershipService,
            ILocalizationService localizationService, ISettingsService settingsService,
            ICategoryService categoryService, ITopicService topicService, IPrivateMessageService privateMessageService,
            IMvcForumContext context)
            : base(loggingService, membershipService, localizationService, settingsService, context)
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
            try
            {
                var membersToDelete = MembershipService.GetUsersByDaysPostsPoints(
                    viewModel.AmoutOfDaysSinceRegistered,
                    viewModel.AmoutOfPosts);
                var count = membersToDelete.Count;
                foreach (var membershipUser in membersToDelete)
                {
                    MembershipService.Delete(membershipUser);
                }
                Context.SaveChanges();
                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = $"{count} members deleted",
                    MessageType = GenericMessages.success
                };
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = ex.Message,
                    MessageType = GenericMessages.danger
                };
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

                Context.SaveChanges();

                categoryFrom.Topics.Clear();

                viewModel.Categories = _categoryService.GetAll();

                Context.SaveChanges();

                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = $"{count} topics moved",
                    MessageType = GenericMessages.success
                };
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = ex.Message,
                    MessageType = GenericMessages.danger
                };
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
                Context.SaveChanges();

                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = $"{count} Private Messages deleted",
                    MessageType = GenericMessages.success
                };
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
                TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = ex.Message,
                    MessageType = GenericMessages.danger
                };
            }


            return View(viewModel);
        }

        #endregion
    }
}