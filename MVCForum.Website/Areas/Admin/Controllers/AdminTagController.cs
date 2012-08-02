using System;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.Areas.Admin.ViewModels;
using MVCForum.Website.Controllers;

namespace MVCForum.Website.Areas.Admin.Controllers
{
    [Authorize(Roles = AppConstants.AdminRoleName)]
    public class AdminTagController : BaseController
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
                var allTags = string.IsNullOrEmpty(search) ? _topicTagService.GetPagedGroupedTags(pageIndex, AppConstants.AdminListPageSize) :
                            _topicTagService.SearchPagedGroupedTags(search, pageIndex, AppConstants.AdminListPageSize);

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

        public ActionResult Manage(int? p, string search)
        {
            return RedirectToAction("Index", new {p, search});
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
                    TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    {
                        Message = string.Format("Delete failed: {0}", ex.Message),
                        MessageType = GenericMessages.error
                    };
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
