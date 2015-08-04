using System.Collections.Generic;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Services
{
    public partial class TopicNotificationService : ITopicNotificationService
    {
        private readonly ITopicNotificationRepository _topicNotificationRepository;
        public TopicNotificationService(ITopicNotificationRepository topicNotificationRepository)
        {
            _topicNotificationRepository = topicNotificationRepository;
        }

        /// <summary>
        /// Return all topic notifications
        /// </summary>
        /// <returns></returns>
        public IList<TopicNotification> GetAll()
        {
            return _topicNotificationRepository.GetAll();
        }

        /// <summary>
        /// Delete topic notification
        /// </summary>
        /// <param name="notification"></param>
        public void Delete(TopicNotification notification)
        {
            _topicNotificationRepository.Delete(notification);
        }

        /// <summary>
        /// Return all notifications for a specified topic
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public IList<TopicNotification> GetByTopic(Topic topic)
        {
            return _topicNotificationRepository.GetByTopic(topic);
        }

        /// <summary>
        /// Return notifications for a specified user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public IList<TopicNotification> GetByUser(MembershipUser user)
        {
            return _topicNotificationRepository.GetByUser(user);
        }

        /// <summary>
        /// return notifications for a specified user on a specified topic
        /// </summary>
        /// <param name="user"></param>
        /// <param name="topic"></param>
        /// <param name="addTracking">If you need to delete these notifications then pass true into addtracking</param>
        /// <returns></returns>
        public IList<TopicNotification> GetByUserAndTopic(MembershipUser user, Topic topic, bool addTracking = false)
        {
            return _topicNotificationRepository.GetByUserAndTopic(user, topic, addTracking);
        }

        /// <summary>
        /// Add a new topic notification
        /// </summary>
        /// <param name="topicNotification"></param>
        public void Add(TopicNotification topicNotification)
        {
            _topicNotificationRepository.Add(topicNotification);
        }
    }
}
