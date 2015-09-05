using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Services
{
    public partial class TagNotificationService : ITagNotificationService
    {
        private readonly ISettingsService _settingsService;
        private readonly ITagNotificationRepository _notificationRepository;

        public TagNotificationService(ISettingsService settingsService, ITagNotificationRepository notificationRepository)
        {
            _settingsService = settingsService;
            _notificationRepository = notificationRepository;
        }

        public IList<TagNotification> GetAll()
        {
            return _notificationRepository.GetAll();
        }

        public void Delete(TagNotification notification)
        {
            _notificationRepository.Delete(notification);
        }

        public IList<TagNotification> GetByTag(TopicTag tag)
        {
            return _notificationRepository.GetByTag(tag);
        }

        public IList<TagNotification> GetByTag(List<TopicTag> tag)
        {
            return _notificationRepository.GetByTag(tag);
        }

        public IList<TagNotification> GetByUser(MembershipUser user)
        {
            return _notificationRepository.GetByUser(user);
        }

        public IList<TagNotification> GetByUserAndTag(MembershipUser user, TopicTag tag, bool addTracking = false)
        {
            return _notificationRepository.GetByUserAndTag(user, tag, addTracking);
        }

        public void Add(TagNotification tagNotification)
        {
            _notificationRepository.Add(tagNotification);
        }
    }
}
