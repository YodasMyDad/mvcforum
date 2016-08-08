namespace MVCForum.Website.Controllers
{
    using System;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using Domain.Constants;
    using Domain.Interfaces.Services;
    using Domain.Interfaces.UnitOfWork;
    using ViewModels;

    public partial class LanguageController : BaseController
    {
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="unitOfWorkManager"> </param>
        /// <param name="membershipService"></param>
        /// <param name="localizationService"></param>
        /// <param name="roleService"></param>
        /// <param name="settingsService"> </param>
        /// <param name="loggingService"> </param>
        /// <param name="cacheService"></param>
        public LanguageController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, ILocalizationService localizationService, 
            IRoleService roleService, ISettingsService settingsService, ICacheService cacheService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService, cacheService)
        {
            
        }

        /// <summary>
        /// Lists out all languages in a partial view. For example, used to display list of 
        /// available languages along the top of every page
        /// </summary>
        /// <returns></returns>
        [ChildActionOnly]
        public ActionResult Index()
        {
            var viewModel = new LanguageListAllViewModel
            {
                Alllanguages = LocalizationService.AllLanguages,
                CurrentLanguage = LocalizationService.CurrentLanguage.Id
            };
            if (viewModel.Alllanguages.Count() <= 1)
            {
                return Content(string.Empty);
            }
            return PartialView("_LanguagePartial", viewModel);
        }

        /// <summary>
        /// Change the current language (typically called from each language link generated in this controller's index method)
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        [HttpPost]
        public ActionResult ChangeLanguage(Guid lang)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var language = LocalizationService.Get(lang);
                LocalizationService.CurrentLanguage = language;

                // The current language is stored in a cookie
                var cookie = new HttpCookie(AppConstants.LanguageIdCookieName)
                {
                    HttpOnly = false,
                    Value = language.Id.ToString(),
                    Expires = DateTime.UtcNow.AddYears(1)
                };

                Response.Cookies.Add(cookie);

                //TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                //{
                //    Message = LocalizationService.GetResourceString("Language.Changed"),
                //    MessageType = GenericMessages.success
                //};

                return Content("success"); 
            }
        }
    }
}
