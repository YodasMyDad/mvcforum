namespace MvcForum.Web.Controllers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Web.Mvc;
    using Core.ExtensionMethods;
    using Core.Interfaces.Services;
    using Core.Interfaces.UnitOfWork;
    using Core.Models.Enums;
    using ViewModels;
    using ViewModels.Tag;

    public partial class TagController : BaseController
    {
        private readonly ICategoryService _categoryService;
        private readonly ITopicTagService _topicTagService;

        public TagController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager,
            IMembershipService membershipService,
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService,
            ITopicTagService topicTagService, ICategoryService categoryService, ICacheService cacheService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService,
                settingsService, cacheService)
        {
            _topicTagService = topicTagService;
            _categoryService = categoryService;
        }

        [ChildActionOnly]
        public PartialViewResult PopularTags(int amountToTake)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var loggedOnReadOnlyUser = User.GetMembershipUser(MembershipService);
                var loggedOnUsersRole = loggedOnReadOnlyUser.GetRole(RoleService);

                var cacheKey = string.Concat("PopularTags", amountToTake, loggedOnUsersRole.Id);
                var viewModel = CacheService.Get<PopularTagViewModel>(cacheKey);
                if (viewModel == null)
                {
                    var allowedCategories = _categoryService.GetAllowedCategories(loggedOnUsersRole);
                    var popularTags = _topicTagService.GetPopularTags(amountToTake, allowedCategories);
                    viewModel = new PopularTagViewModel {PopularTags = popularTags};
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