using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public interface ITopicNotificationService
    {
        IList<TopicNotification> GetAll();
        void Delete(TopicNotification notification);
        IList<TopicNotification> GetByTopic(Topic topic);
        IList<TopicNotification> GetByUser(MembershipUser user);
        IList<TopicNotification> GetByUserAndTopic(MembershipUser user, Topic topic);
        void Add(TopicNotification topicNotification);
    }
}
