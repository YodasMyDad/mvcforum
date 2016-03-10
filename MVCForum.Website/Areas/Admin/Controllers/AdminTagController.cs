﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.Application;
using MVCForum.Website.Areas.Admin.ViewModels;
using MVCForum.Website.Controllers;

namespace MVCForum.Website.Areas.Admin.Controllers
{
    [Authorize(Roles = AppConstants.AdminRoleName)]
    public partial class AdminTagController : BaseController
    {
        private readonly ITopicTagService _topicTagService;

        public AdminTagController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService, ITopicTagService topicTagService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _topicTagService = topicTagService;
        }

        public ActionResult Index(int? p, string search)
        {
            var pageIndex = p ?? 1;

            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var allTags = string.IsNullOrEmpty(search) ? _topicTagService.GetPagedGroupedTags(pageIndex, SiteConstants.Instance.AdminListPageSize) :
                            _topicTagService.SearchPagedGroupedTags(search, pageIndex, SiteConstants.Instance.AdminListPageSize);

                var memberListModel = new ListTagsViewModel
                {
                    Tags = allTags,
                    PageIndex = pageIndex,
                    TotalCount = allTags.TotalCount,
                    Search = search
                };

                return View(memberListModel);
            }

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
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var viewModel = new MoveTagsViewModel
                                {
                                    Tags = TagsSelectList()
                                };
                return View(viewModel);
            }
        }

        [HttpPost]
        public ActionResult MoveTags(MoveTagsViewModel viewModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
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
                    unitOfWork.Commit();
                    ShowMessage(new GenericMessageViewModel
                    {
                        Message = string.Format("All topics tagged with {0} have been updated to {1}", oldTag.Tag, newTag.Tag),
                        MessageType = GenericMessages.success
                    });
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    ShowMessage(new GenericMessageViewModel
                    {
                        Message = string.Format("Error: {0}", ex.Message),
                        MessageType = GenericMessages.danger
                    });
                }

                return View(viewModel); 
            }
        }


        public ActionResult Manage(int? p, string search)
        {
            return RedirectToAction("Index", new { p, search });
        }


        public ActionResult Delete(string tag)
        {


            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                _topicTagService.DeleteByName(tag);

                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = "Tags delete successfully",
                    MessageType = GenericMessages.success
                };

                try
                {
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    ShowMessage(new GenericMessageViewModel
                    {
                        Message = string.Format("Delete failed: {0}", ex.Message),
                        MessageType = GenericMessages.danger
                    });
                }

            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        public void UpdateTag(AjaxEditTagViewModel viewModel)
        {
            if (Request.IsAjaxRequest())
            {
                using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
                {
                    _topicTagService.UpdateTagNames(viewModel.NewName, viewModel.OldName);

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

    }
}
