using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.Areas.Admin.ViewModels;

namespace MVCForum.Website.Areas.Admin.Controllers
{
    [Authorize(Roles = AppConstants.AdminRoleName)]
    public partial class ModerateController : BaseAdminController
    {
        public IActivityService _activityService { get; set; }
        private readonly IRoleService _roleService;
        private readonly ILuceneService _luceneService;
        private readonly IPostService _postService;
        private readonly ITopicService _topicService;
        private const int ModeratePageSize = 15;

        public ModerateController(ILoggingService loggingService,
            IUnitOfWorkManager unitOfWorkManager,
            IMembershipService membershipService,
            ILocalizationService localizationService,
            IRoleService roleService,
            ISettingsService settingsService, IPostService postService, ITopicService topicService, ILuceneService luceneService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, settingsService)
        {
            _topicService = topicService;
            _luceneService = luceneService;
            _postService = postService;
            _roleService = roleService;
        }

        public ActionResult Index()
        {
            // Show both pending topics and also pending posts
            // Use ajax for paging too
            var viewModel = new AdminModerateViewModel
            {
                Posts = _postService.GetPagedPendingPosts(1, ModeratePageSize),
                Topics = _topicService.GetPagedPendingTopics(1, ModeratePageSize)
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult GetMoreTopics(AjaxPagingViewModel viewModel)
        {
            var topics = _topicService.GetPagedPendingTopics(viewModel.PageIndex, ModeratePageSize);
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
                    var postIdList = new List<Guid>();

                    if (viewModel.IsApproved)
                    {
                        topic.Pending = false;
                    }
                    else
                    {
                        var postId = topic.LastPost.Id;
                        var post = _postService.Get(postId);
                        var deleteTopic = _postService.Delete(post);

                        unitOfWork.SaveChanges();

                        if (deleteTopic)
                        {
                            postIdList = topic.Posts.Select(x => x.Id).ToList();
                            _topicService.Delete(topic);
                        }
                    }

                    // We use temp data because we are doing a redirect
                    //TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                    //{
                    //    Message = "Category Created",
                    //    MessageType =
                    //        GenericMessages.success
                    //};
                    unitOfWork.Commit();

                    // Delete the topic posts if 
                    if (_luceneService.CheckIndexExists() && !viewModel.IsApproved)
                    {
                        foreach (var guid in postIdList)
                        {
                            _luceneService.Delete(guid);
                        }
                    }

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
                    var postId = post.Id;

                    if (viewModel.IsApproved)
                    {
                        post.Pending = false;
                    }
                    else
                    {
                        _postService.Delete(post);
                    }

                    unitOfWork.Commit();

                    if (_luceneService.CheckIndexExists() && !viewModel.IsApproved)
                    {
                        _luceneService.Delete(postId);
                    }

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