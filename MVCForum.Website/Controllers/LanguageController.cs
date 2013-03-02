using System;
using System.Web;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.Areas.Admin.ViewModels;
using MVCForum.Website.ViewModels;

namespace MVCForum.Website.Controllers
{
    public class LanguageController : BaseController
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
        public LanguageController(ILoggingService loggingService, IUnitOfWorkManager unitOfWorkManager, IMembershipService membershipService, ILocalizationService localizationService, 
            IRoleService roleService, ISettingsService settingsService)
            : base(loggingService, unitOfWorkManager, membershipService, localizationService, roleService, settingsService)
        {
            
        }

        //
        // GET: /Language/

        /// <summary>
        /// Lists out all languages in a partial view. For example, used to display list of 
        /// available languages along the top of every page
        /// </summary>
        /// <returns></returns>
        [ChildActionOnly]
        public PartialViewResult Index()
        {
            var viewModel = new LanguageListAllViewModel{Alllanguages = LocalizationService.AllLanguages};
            return PartialView("_LanguagePartial", viewModel);
        }

        /// <summary>
        /// Change the current language (typically called from each language link generated in this controller's index method)
        /// </summary>
        /// <param name="lang"></param>
        /// <returns></returns>
        public ActionResult ChangeLanguage(Guid lang)
        {
            using (UnitOfWorkManager.NewUnitOfWork())
            {
                var language = LocalizationService.Get(lang);
                LocalizationService.CurrentLanguage = language;

                // The current language is stored in a cookie
                var cookie = new HttpCookie(AppConstants.LanguageCultureCookieName)
                {
                    HttpOnly = false,
                    Value = language.LanguageCulture,
                    Expires = DateTime.UtcNow.AddYears(1)
                };

                Response.Cookies.Add(cookie);

                TempData[AppConstants.MessageViewBagName] = new GenericMessageViewModel
                {
                    Message = LocalizationService.GetResourceString("Language.Changed"),
                    MessageType = GenericMessages.success
                };

                return RedirectToAction("Index", "Home"); 
            }
        }
    }
}
