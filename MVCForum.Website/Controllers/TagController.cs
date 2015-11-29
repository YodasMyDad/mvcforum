using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel.Enums;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Utilities;
using MVCForum.Website.Application;
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
                var cacheKey = string.Concat("PopularTags", amountToTake, UsersRole.Id);
                var viewModel = _cacheService.Get<PopularTagViewModel>(cacheKey);
                if (viewModel == null)
                {
                    var allowedCategories = _categoryService.GetAllowedCategories(UsersRole);
                    var popularTags = _topicTagService.GetPopularTags(amountToTake, allowedCategories);
                    viewModel = new PopularTagViewModel { PopularTags = popularTags };
                    _cacheService.Set(cacheKey, viewModel, CacheTimes.SixHours);
                }
                return PartialView(viewModel);
            }
        }

        [HttpGet]
        public JsonResult AutoCompleteTags(string term)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var toReturn = string.Empty;
                var returnList = new List<string>();
                var tags = _topicTagService.GetContains(term);

                if (!tags.Any())
                {
                    return Json(returnList, JsonRequestBehavior.AllowGet);
                }

                foreach (var topicTag in tags)
                {
                    toReturn += string.Format("\"{0}\",", topicTag.Tag);
                    returnList.Add(topicTag.Tag);
                }

                return Json(returnList, JsonRequestBehavior.AllowGet);
                
            }
        }

    }
}
