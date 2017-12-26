namespace MvcForum.Web.Controllers
{
    using System.Web.Mvc;
    using Core.DomainModel.Enums;
    using Core.Interfaces.Services;
    using Core.Interfaces.UnitOfWork;
    using ViewModels;

    public partial class StatsController : BaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly IPostService _postService;
        private readonly ITopicService _topicService;

        public StatsController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager,
            IMembershipService membershipService,
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService,
            ITopicService topicService,
            IPostService postService, ICategoryService categoryService, ICacheService cacheService) :
            base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService,
                settingsService, cacheService)
        {
            _topicService = topicService;
            _postService = postService;
            _categoryService = categoryService;
        }

        [ChildActionOnly]
        [OutputCache(Duration = (int) CacheTimes.OneHour)]
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