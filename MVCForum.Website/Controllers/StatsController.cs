namespace MVCForum.Website.Controllers
{
    using System.Web.Mvc;
    using Domain.DomainModel.Enums;
    using Domain.Interfaces.Services;
    using Domain.Interfaces.UnitOfWork;
    using ViewModels;

    public partial class StatsController : BaseController
    {
        private readonly ITopicService _topicService;
        private readonly IPostService _postService;
        private readonly ICategoryService _categoryService;

        public StatsController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, 
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService, ITopicService topicService, 
            IPostService postService, ICategoryService categoryService, ICacheService cacheService) : 
            base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService, cacheService)
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
