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
            settings.AdminEmailAddress = StringUtils.SafePlainText(settings.AdminEmailAddress);
            settings.AkismentKey = StringUtils.SafePlainText(settings.AkismentKey);
            settings.CurrentDatabaseVersion = StringUtils.SafePlainText(settings.CurrentDatabaseVersion);
            settings.ForumName = StringUtils.SafePlainText(settings.ForumName);
            settings.ForumUrl = StringUtils.SafePlainText(settings.ForumUrl);
            settings.NotificationReplyEmail = StringUtils.SafePlainText(settings.NotificationReplyEmail);
            settings.SMTP = StringUtils.SafePlainText(settings.SMTP);
            settings.SMTPPassword = StringUtils.SafePlainText(settings.SMTPPassword);
            settings.SMTPPort = StringUtils.SafePlainText(settings.SMTPPort);
            settings.SMTPUsername = StringUtils.SafePlainText(settings.SMTPUsername);
            settings.SpamAnswer = StringUtils.SafePlainText(settings.SpamAnswer);
            settings.SpamQuestion = StringUtils.SafePlainText(settings.SpamQuestion);
            _settingsRepository.Update(settings);
        }

        public Settings Add(Settings settings)
        {
            return _settingsRepository.Add(settings);
        }
    }
}
