using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.ViewModels;

namespace MVCForum.Website.Controllers
{
    public partial class TagController : BaseController
    {
        private readonly ITopicTagService _topicTagService;
        private readonly ICategoryService _categoryService;
        private readonly ICacheService _cacheService;

        public TagController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService, ITopicTagService topicTagService, ICategoryService categoryService, ICacheService cacheService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            _topicTagService = topicTagService;
            _categoryService = categoryService;
            _cacheService = cacheService;
        }

        [ChildActionOnly]
        public PartialViewResult PopularTags(int amountToTake)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                PopularTagViewModel viewModel;
                var cacheKey = string.Concat("PopularTags", amountToTake, UsersRole.Id);
                var cachedData = _cacheService.Get(cacheKey);
                if (cachedData == null)
                {
                    var allowedCategories = _categoryService.GetAllowedCategories(UsersRole);
                    var popularTags = _topicTagService.GetPopularTags(amountToTake, allowedCategories);
                    viewModel = new PopularTagViewModel { PopularTags = popularTags };   
                    _cacheService.Set(cacheKey, viewModel, AppConstants.LongCacheTime);
                }
                else
                {
                    viewModel = (PopularTagViewModel) cachedData;
                }
                return PartialView(viewModel);
            }
        }

    }
}
