using System.Web.Mvc;
using MVCForum.Domain.DomainModel.Enums;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.ViewModels;

namespace MVCForum.Website.Controllers
{
    public partial class StatsController : BaseController
    {
        private readonly ITopicService _topicService;
        private readonly IPostService _postService;
        private readonly ICategoryService _categoryService;

        public StatsController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, 
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService, ITopicService topicService, 
            IPostService postService, ICategoryService categoryService) : 
            base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _topicService = topicService;
            _postService = postService;
            _categoryService = categoryService;
        }

        [ChildActionOnly]
        [OutputCache(Duration = (int)CacheTimes.OneHour)]
        public PartialViewResult GetMainStats()
        {
            var allCats = _categoryService.GetAll();
            var viewModel = new MainStatsViewModel
                                {
                                    LatestMembers = MembershipService.GetLatestUsers(10),
                                    MemberCount = MembershipService.MemberCount(),
                                    TopicCount = _topicService.TopicCount(allCats),
                                    PostCount = _postService.PostCount(allCats)
                                };
            return PartialView(viewModel);
        }

    }
}
