using System;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.ViewModels;

namespace MVCForum.Website.Controllers
{
    [Authorize]
    public partial class ModerateController : BaseController
    {
        private readonly IPostService _postService;
        private readonly ITopicService _topicService;
        private readonly ICategoryService _categoryService;

        public ModerateController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, 
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService, IPostService postService, 
            ITopicService topicService, ICategoryService categoryService) 
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _postService = postService;
            _topicService = topicService;
            _categoryService = categoryService;
        }

        public ActionResult Index()
        {
            // Show both pending topics and also pending posts
            // Use ajax for paging too
            var allowedCategories = _categoryService.GetAllowedCategories(UsersRole);
            var viewModel = new ModerateViewModel
            {
                Posts = _postService.GetPendingPosts(allowedCategories, UsersRole),
                Topics = _topicService.GetPendingTopics(allowedCategories, UsersRole)
            };
            return View(viewModel);
        }

        [HttpPost]
        public ActionResult ModerateTopic(ModerateActionViewModel viewModel)
        {
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    var topic = _topicService.Get(viewModel.TopicId);
                    var permissions = RoleService.GetPermissions(topic.Category, UsersRole);

                    // Is this user allowed to moderate - We use EditPosts for now until we change the permissions system
                    if (!permissions[SiteConstants.Instance.PermissionEditPosts].IsTicked)
                    {
                        return Content(LocalizationService.GetResourceString("Errors.NoPermission"));
                    }

                    if (viewModel.IsApproved)
                    {
                        topic.Pending = false;
                    }
                    else
                    {
                        _topicService.Delete(topic, unitOfWork);
                    }

                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    return Content(ex.Message);
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
                    var permissions = RoleService.GetPermissions(post.Topic.Category, UsersRole);
                    if (!permissions[SiteConstants.Instance.PermissionEditPosts].IsTicked)
                    {
                        return Content(LocalizationService.GetResourceString("Errors.NoPermission"));
                    }

                    if (viewModel.IsApproved)
                    {
                        post.Pending = false;
                    }
                    else
                    {
                        _postService.Delete(post, unitOfWork, false);
                    }

                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    unitOfWork.Rollback();
                    LoggingService.Error(ex);
                    return Content(ex.Message);
                }
            }

            return Content("allgood");
        }


    }
}