namespace MvcForum.Web.Areas.Admin.Controllers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Core;
    using Core.Constants;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Core.Models.Entities;
    using Web.ViewModels;
    using Web.ViewModels.Admin;

    [Authorize(Roles = Constants.AdminRoleName)]
    public class AdminTagController : BaseAdminController
    {
        private readonly ITopicTagService _topicTagService;

        public AdminTagController(ILoggingService loggingService, IMembershipService membershipService,
            ILocalizationService localizationService, ISettingsService settingsService,
            ITopicTagService topicTagService, IMvcForumContext context)
            : base(loggingService, membershipService, localizationService, settingsService, context)
        {
            _topicTagService = topicTagService;
        }

        public async Task<ActionResult> Index(int? p, string search)
        {
            var pageIndex = p ?? 1;


            var allTags = string.IsNullOrWhiteSpace(search)
                ? await _topicTagService.GetPagedGroupedTags(pageIndex, ForumConfiguration.Instance.AdminListPageSize)
                : await _topicTagService.SearchPagedGroupedTags(search, pageIndex,
                    ForumConfiguration.Instance.AdminListPageSize);

            var memberListModel = new ListTagsViewModel
            {
                Tags = allTags,
                PageIndex = pageIndex,
                TotalCount = allTags.TotalCount,
                Search = search
            };

            return View(memberListModel);
        }

        private List<SelectListItem> TagsSelectList()
        {
            var list = new List<SelectListItem>();
            foreach (var tag in _topicTagService.GetAll().OrderBy(x => x.Tag).ToList())
            {
                list.Add(new SelectListItem
                {
                    Text = tag.Tag,
                    Value = tag.Id.ToString()
                });
            }
            return list;
        }

        public ActionResult MoveTags()
        {
            var viewModel = new MoveTagsViewModel
            {
                Tags = TagsSelectList()
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult MoveTags(MoveTagsViewModel viewModel)
        {
            var oldTag = _topicTagService.Get(viewModel.CurrentTagId);
            var newTag = _topicTagService.Get(viewModel.NewTagId);

            // Look through the topics and add the new tag to it and remove the old!                
            var topics = new List<Topic>();
            topics.AddRange(oldTag.Topics);
            foreach (var topic in topics)
            {
                topic.Tags.Remove(oldTag);
                topic.Tags.Add(newTag);
            }

            // Reset the tags
            viewModel.Tags = TagsSelectList();

            try
            {
                Context.SaveChanges();
                ShowMessage(new GenericMessageViewModel
                {
                    Message = $"All topics tagged with {oldTag.Tag} have been updated to {newTag.Tag}",
                    MessageType = GenericMessages.success
                });
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
                ShowMessage(new GenericMessageViewModel
                {
                    Message = $"Error: {ex.Message}",
                    MessageType = GenericMessages.danger
                });
            }

            return View(viewModel);
        }


        public ActionResult Manage(int? p, string search)
        {
            return RedirectToAction("Index", new {p, search});
        }


        public ActionResult Delete(string tag, int? p)
        {
            var page = p ?? 1;

            _topicTagService.DeleteByName(tag);

            TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
            {
                Message = "Tags delete successfully",
                MessageType = GenericMessages.success
            };

            try
            {
                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
                ShowMessage(new GenericMessageViewModel
                {
                    Message = $"Delete failed: {ex.Message}",
                    MessageType = GenericMessages.danger
                });
            }
                        
            return RedirectToAction("Index",  new { p = page });
        }

        [HttpPost]
        public void UpdateTag(AjaxEditTagViewModel viewModel)
        {
            if (Request.IsAjaxRequest())
            {
                _topicTagService.UpdateTagNames(viewModel.NewName, viewModel.OldName);

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
    }
}