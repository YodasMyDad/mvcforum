namespace MVCForum.Website.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using Domain.DomainModel.Enums;
    using Domain.Interfaces.Services;
    using Domain.Interfaces.UnitOfWork;
    using ViewModels;

    public partial class TagController : BaseController
    {
        private readonly ITopicTagService _topicTagService;
        private readonly ICategoryService _categoryService;

        public TagController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, 
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService, ITopicTagService topicTagService, ICategoryService categoryService, ICacheService cacheService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService, cacheService)
        {
            _topicTagService = topicTagService;
            _categoryService = categoryService;
        }

        [ChildActionOnly]
        public PartialViewResult PopularTags(int amountToTake)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var cacheKey = string.Concat("PopularTags", amountToTake, UsersRole.Id);
                var viewModel = CacheService.Get<PopularTagViewModel>(cacheKey);
                if (viewModel == null)
                {
                    var allowedCategories = _categoryService.GetAllowedCategories(UsersRole);
                    var popularTags = _topicTagService.GetPopularTags(amountToTake, allowedCategories);
                    viewModel = new PopularTagViewModel { PopularTags = popularTags };
                    CacheService.Set(cacheKey, viewModel, CacheTimes.SixHours);
                }
                return PartialView(viewModel);
            }
        }

        [HttpGet]
        public JsonResult AutoCompleteTags(string term)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var returnList = new List<string>();
                var tags = _topicTagService.GetContains(term);

                if (!tags.Any())
                {
                    return Json(returnList, JsonRequestBehavior.AllowGet);
                }

                foreach (var topicTag in tags)
                {
                    returnList.Add(topicTag.Tag);
                }

                return Json(returnList, JsonRequestBehavior.AllowGet);
                
            }
        }

    }
}
