using System.Web;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Utilities;

namespace MVCForum.Services
{
    public class SettingsService : ISettingsService
    {
        private readonly ISettingsRepository _settingsRepository;

        public SettingsService(ISettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        /// <summary>
        /// Get the site settings from cache, if not in cache gets from database and adds into the cache
        /// </summary>
        /// <returns></returns>
        public Settings GetSettings(bool useCache = true)
        {
            //TODO  Testing for performance, assign settings per request to save hits and making hashcodes constantly from same request for settings
            if (useCache)
            {
                var objectContextKey = HttpContext.Current.GetHashCode().ToString("x");
                if (!HttpContext.Current.Items.Contains(objectContextKey))
                {
                    HttpContext.Current.Items.Add(objectContextKey, _settingsRepository.GetSettings());
                }
                return HttpContext.Current.Items[objectContextKey] as Settings;   
            }
            return _settingsRepository.GetSettings();
        }

        /// <summary>
        /// Save settings (Clears cache upon save)
        /// </summary>
        /// <param name="settings"></param>
        public void Save(Settings settings)
        {
            _settingsRepository.Update(settings);
        }
    }
}
