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
            //// Use our own cache here to improve performance
            //if(useCache)
            //{
            //    Settings settings;
            //    if (!CacheUtils.Get(AppConstants.SettingsCacheName, out settings))
            //    {
            //        settings = _settingsRepository.GetSettings();
            //        CacheUtils.Add(settings, AppConstants.SettingsCacheName);
            //    }
            //    return settings;   
            //}

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
