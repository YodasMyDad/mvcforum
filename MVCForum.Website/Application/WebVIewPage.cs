using MVCForum.Domain;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Website.Application.Localization;
using Ninject;

namespace MVCForum.Website.Application
{
    /// <summary>
    /// WebViewPage is used as the base class when a View is rendered into a class. 
    /// We are extending the class with extra functionality, thereby extending Razor
    /// </summary>
    public abstract class WebViewPage<TModel> : System.Web.Mvc.WebViewPage<TModel>
    {
        [Inject]
        public ISettingsService SettingsService { get; set; }

        [Inject]
        public ILocalizationService LocalizationService { get; set; }

        /// <summary>
        /// Default method used in Views to resolve localized strings
        /// </summary>
        /// <param name="format"></param>
        /// <param name="args"></param>
        /// <returns></returns>
        public LocalizedString LocalizerMethod(string format, object[] args)
        {
            var resFormat = LocalizationService.GetResource(LocalizationService.CurrentLanguage, format.Trim());
            if(resFormat != null)
            {
                var resValue = resFormat.ResourceValue;
                if (string.IsNullOrEmpty(resValue))
                {
                    return new LocalizedString(format);
                }

                return new LocalizedString((args == null || args.Length == 0)
                                            ? resValue
                                            : string.Format(resValue, args));
            }
            return null;
        }

        /// <summary>
        /// Public method available to Views, e.g. @T("StringKey")
        /// </summary>
        public Localizer T
        {
            get { return LocalizerMethod; }
        }

    }

    public abstract class WebViewPage : WebViewPage<dynamic>
    {
    }
}