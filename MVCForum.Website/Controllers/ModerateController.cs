namespace MvcForum.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Core;
    using Core.ExtensionMethods;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using ViewModels;
    using ViewModels.Moderate;

    [Authorize]
    public partial class ModerateController : BaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly IPostService _postService;
        private readonly ITopicService _topicService;
        private readonly IActivityService _activityService;

        public ModerateController(ILoggingService loggingService, IMembershipService membershipService,
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService,
            IPostService postService, ITopicService topicService, ICategoryService categoryService,
            ICacheService cacheService, IMvcForumContext context, IActivityService activityService)
            : base(loggingService, membershipService, localizationService, roleService,
                settingsService, cacheService, context)
        {
            _postService = postService;
            _topicService = topicService;
            _categoryService = categoryService;
            _activityService = activityService;
        }

        public virtual ActionResult Index()
        {
            var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
            var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

            // Show both pending topics and also pending posts
            // Use ajax for paging too
            var allowedCategories = _categoryService.GetAllowedCategories(loggedOnUsersRole);
            var viewModel = new ModerateViewModel
            {
                Posts = _postService.GetPendingPosts(allowedCategories, loggedOnUsersRole),
                Topics = _topicService.GetPendingTopics(allowedCategories, loggedOnUsersRole)
            };
            return View(viewModel);
        }

        [HttpPost]
        public virtual async Task<ActionResult> ModerateTopic(ModerateActionViewModel viewModel)
        {
            try
            {
                var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
                var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

                var topic = _topicService.Get(viewModel.TopicId);
                var permissions = RoleService.GetPermissions(topic.Category, loggedOnUsersRole);

                // Is this user allowed to moderate - We use EditPosts for now until we change the permissions system
                if (!permissions[ForumConfiguration.Instance.PermissionEditPosts].IsTicked)
                {
                    return Content(LocalizationService.GetResourceString("Errors.NoPermission"));
                }

                if (viewModel.IsApproved)
                {
                    topic.Pending = false;
                    _activityService.TopicCreated(topic);
                }
                else
                {
                    var topicResult = await _topicService.Delete(topic);
                    if (!topicResult.Successful)
                    {
                        return Content(topicResult.ProcessLog.FirstOrDefault());
                    }
                }

                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
                return Content(ex.Message);
            }


            return Content("allgood");
        }

        [HttpPost]
        public virtual ActionResult ModeratePost(ModerateActionViewModel viewModel)
        {
            try
            {
                var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
                var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

                var post = _postService.Get(viewModel.PostId);
                var permissions = RoleService.GetPermissions(post.Topic.Category, loggedOnUsersRole);
                if (!permissions[ForumConfiguration.Instance.PermissionEditPosts].IsTicked)
                {
                    return Content(LocalizationService.GetResourceString("Errors.NoPermission"));
                }

                if (viewModel.IsApproved)
                {
                    post.Pending = false;
                    _activityService.PostCreated(post);
                }
                else
                {
                    _postService.Delete(post, false);
                }

                Context.SaveChanges();
            }
            catch (Exception ex)
            {
                Context.RollBack();
                LoggingService.Error(ex);
                return Content(ex.Message);
            }


            return Content("allgood");
        }
    }
}