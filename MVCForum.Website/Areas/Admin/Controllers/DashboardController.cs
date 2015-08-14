using System.Linq;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Utilities;
using MVCForum.Website.Areas.Admin.ViewModels;

namespace MVCForum.Website.Areas.Admin.Controllers
{
    [Authorize(Roles = AppConstants.AdminRoleName)]
    public partial class DashboardController : BaseAdminController
    {
        private readonly IPostService _postService;
        private readonly ITopicService _topicService;
        private readonly ITopicTagService _topicTagService;
        private readonly ICategoryService _categoryService;
        private readonly IMembershipUserPointsService _membershipUserPointsService;
        const int AmountToShow = 7;

        public DashboardController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService,
            ILocalizationService localizationService, ISettingsService settingsService, IPostService postService, 
            ITopicService topicService, ITopicTagService topicTagService, IMembershipUserPointsService membershipUserPointsService, ICategoryService categoryService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, settingsService)
        {
            _membershipUserPointsService = membershipUserPointsService;
            _categoryService = categoryService;
            _postService = postService;
            _topicService = topicService;
            _topicTagService = topicTagService;
        }

        [HttpPost]
        public PartialViewResult TodaysTopics()
        {
            // Get all cats as only admins can view this page
            var allCats = _categoryService.GetAll().ToList();

            if (Request.IsAjaxRequest())
            {
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    return PartialView(new TodaysTopics { Topics = _topicService.GetTodaysTopics(AmountToShow, allCats) });
                }
            }
            return null;
        }

        [HttpPost]
        public PartialViewResult LatestUsers()
        {
            if (Request.IsAjaxRequest())
            {
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    return PartialView(new LatestUsersViewModels { Users = MembershipService.GetLatestUsers(AmountToShow) });
                }
            }
            return null;
        }

        [HttpPost]
        public PartialViewResult LowestPointUsers()
        {
            if (Request.IsAjaxRequest())
            {
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    return PartialView(new LowestPointUsersViewModels { Users = _membershipUserPointsService.GetAllTimePointsNegative(AmountToShow) });
                }
            }
            return null;
        }

        [HttpPost]
        public PartialViewResult LowestPointPosts()
        {
            if (Request.IsAjaxRequest())
            {
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    return PartialView(new LowestPointPostsViewModels { Posts = _postService.GetLowestVotedPost(AmountToShow) });
                }
            }
            return null;
        }

        [HttpPost]
        public PartialViewResult HighestViewedTopics()
        {
            if (Request.IsAjaxRequest())
            {
                // Get all cats as only admins can view this page
                var allCats = _categoryService.GetAll().ToList();
                using (UnitOfWorkManager.NewUnitOfWork())
                {
                    return PartialView(new HighestViewedTopics { Topics = _topicService.GetHighestViewedTopics(AmountToShow, allCats) });
                }
            }
            return null;
        }

        [HttpPost]
        public PartialViewResult MvcForumLatestNews()
        {
            if (Request.IsAjaxRequest())
            {
                    var reader = new RssReader();
                    var viewModel = new LatestNewsViewModel { RssFeed = reader.GetRssFeed("http://www.mvcforum.com/rss").Take(AmountToShow).ToList() };
                    return PartialView(viewModel);
            }
            return null;
        }
    }
}
