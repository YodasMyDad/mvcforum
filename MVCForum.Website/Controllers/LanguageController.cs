namespace MvcForum.Web.Controllers
{
    using System;
    using System.Linq;
    using System.Web;
    using System.Web.Mvc;
    using Core.Constants;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using ViewModels.Language;

    public partial class LanguageController : BaseController
    {
        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="loggingService"> </param>
        /// <param name="membershipService"></param>
        /// <param name="localizationService"></param>
        /// <param name="roleService"></param>
        /// <param name="settingsService"> </param>
        /// <param name="cacheService"></param>
        /// <param name="context"></param>
        public LanguageController(ILoggingService loggingService, IMembershipService membershipService,
            ILocalizationService localizationService, IRoleService roleService, ISettingsService settingsService,
            ICacheService cacheService, IMvcForumContext context)
            : base(loggingService, membershipService, localizationService, roleService,
                settingsService, cacheService, context)
        {
        }

        /// <summary>
        ///     Lists out all languages in a partial view. For example, used to display list of
        ///     available languages along the top of every page
        /// </summary>
        /// <returns></returns>
        [ChildActionOnly]
        public virtual ActionResult Index()
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
        ///     Change the current language (typically called from each language link generated in this controller's index method)
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        [HttpPost]
        public virtual ActionResult ChangeLanguage(Guid lang)
        {
            var language = LocalizationService.Get(lang);
            LocalizationService.CurrentLanguage = language;

            // The current language is stored in a cookie
            var cookie = new HttpCookie(Constants.LanguageIdCookieName)
            {
                HttpOnly = false,
                Value = language.Id.ToString(),
                Expires = DateTime.UtcNow.AddYears(1)
            };

            Response.Cookies.Add(cookie);

            //TempData[Constants.MessageViewBagName] = new GenericMessageViewModel
            //{
            //    Message = LocalizationService.GetResourceString("Language.Changed"),
            //    MessageType = GenericMessages.success
            //};

            return Content("success");
        }
    }
}