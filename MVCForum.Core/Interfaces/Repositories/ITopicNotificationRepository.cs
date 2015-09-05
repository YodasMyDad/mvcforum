using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public partial interface ITopicNotificationRepository
    {
        IList<TopicNotification> GetAll();
        IList<TopicNotification> GetByTopic(Topic topic);
        IList<TopicNotification> GetByUser(MembershipUser user);
        IList<TopicNotification> GetByUserAndTopic(MembershipUser user, Topic category, bool addTracking = false);
        TopicNotification Add(TopicNotification item);
        TopicNotification Get(Guid id);
        void Delete(TopicNotification item);
    }
}
