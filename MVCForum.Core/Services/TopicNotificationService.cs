namespace MvcForum.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using Data.Context;
    using Interfaces;
    using Interfaces.Services;
    using Models.Entities;

    public partial class TopicNotificationService : ITopicNotificationService
    {
        private readonly MvcForumContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"> </param>
        public TopicNotificationService(IMvcForumContext context)
        {
            _context = context as MvcForumContext;
        }

        /// <summary>
        /// Return all topic notifications
        /// </summary>
        /// <returns></returns>
        public IList<TopicNotification> GetAll()
        {
            return _context.TopicNotification.ToList();
        }

        /// <summary>
        /// Delete topic notification
        /// </summary>
        /// <param name="notification"></param>
        public void Delete(TopicNotification notification)
        {
            _context.TopicNotification.Remove(notification);
        }

        /// <summary>
        /// Return all notifications for a specified topic
        /// </summary>
        /// <param name="topic"></param>
        /// <returns></returns>
        public IList<TopicNotification> GetByTopic(Topic topic)
        {
            return _context.TopicNotification
                .Where(x => x.Topic.Id == topic.Id)
                .AsNoTracking()
                .ToList();
        }

        /// <summary>
        /// Return notifications for a specified user
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public IList<TopicNotification> GetByUser(MembershipUser user)
        {
            return _context.TopicNotification
                .Where(x => x.User.Id == user.Id)
                .ToList();
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
            var notifications = _context.TopicNotification
                .Where(x => x.User.Id == user.Id && x.Topic.Id == topic.Id);
            if (addTracking)
            {
                return notifications.ToList();
            }
            return notifications.AsNoTracking().ToList();
        }

        /// <summary>
        /// Add a new topic notification
        /// </summary>
        /// <param name="topicNotification"></param>
        public TopicNotification Add(TopicNotification topicNotification)
        {
            return _context.TopicNotification.Add(topicNotification);
        }

        public TopicNotification Get(Guid id)
        {
            return _context.TopicNotification.FirstOrDefault(x => x.Id == id);
        }
    }
}
