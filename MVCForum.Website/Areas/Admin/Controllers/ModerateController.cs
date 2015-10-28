using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.Areas.Admin.ViewModels;

namespace MVCForum.Website.Areas.Admin.Controllers
{
    [Authorize(Roles = AppConstants.AdminRoleName)]
    public partial class ModerateController : BaseAdminController
    {
        private readonly IRoleService _roleService;
        private readonly IPostService _postService;
        private readonly ITopicService _topicService;
        private readonly ICategoryService _categoryService;
        private readonly List<Category> _allCategories;

        private const int ModeratePageSize = 15;

        public ModerateController(ILoggingService loggingService,
            IUnitOfWorkManager unitOfWorkManager,
            IMembershipService membershipService,
            ILocalizationService localizationService,
            IRoleService roleService,
            ISettingsService settingsService, IPostService postService, ITopicService topicService, ICategoryService categoryService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, settingsService)
        {
            _topicService = topicService;
            _categoryService = categoryService;
            _postService = postService;
            _roleService = roleService;

            _allCategories = _categoryService.GetAll();
        }

        public ActionResult Index()
        {
            // Show both pending topics and also pending posts
            // Use ajax for paging too
            var viewModel = new AdminModerateViewModel
            {
                Posts = _postService.GetPagedPendingPosts(1, ModeratePageSize),
                Topics = _topicService.GetPagedPendingTopics(1, ModeratePageSize, _allCategories)
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult GetMoreTopics(AjaxPagingViewModel viewModel)
        {
            var topics = _topicService.GetPagedPendingTopics(viewModel.PageIndex, ModeratePageSize, _allCategories);
            return View(topics);
        }

        [HttpPost]
        public ActionResult GetMorePosts(AjaxPagingViewModel viewModel)
        {
            var posts = _postService.GetPagedPendingPosts(viewModel.PageIndex, ModeratePageSize);
            return View(posts);
        }

        [HttpPost]
        public ActionResult ModerateTopic(ModerateActionViewModel viewModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {

                try
                {
                    var topic = _topicService.Get(viewModel.TopicId);

                    if (viewModel.IsApproved)
                    {
                        topic.Pending = false;
                    }
                    else
                    {
                        var postId = topic.LastPost.Id;
                        var post = _postService.Get(postId);
                        _postService.Delete(post, unitOfWork);

                        unitOfWork.SaveChanges();
                    }

                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    return Content("error");
                }
            }

            return Content("allgood");
        }

        [HttpPost]
        public ActionResult ModeratePost(ModerateActionViewModel viewModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {

                try
                {
                    var post = _postService.Get(viewModel.PostId);

                    if (viewModel.IsApproved)
                    {
                        post.Pending = false;
                    }
                    else
                    {
                        _postService.Delete(post, unitOfWork);
                    }

                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    return Content("error");
                }
            }

            return Content("allgood");
        }
    }
}