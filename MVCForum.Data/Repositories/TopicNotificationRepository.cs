using System;
using System.Collections.Generic;
using System.Linq;
using MVCForum.Data.Context;
using System.Data.Entity;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;

namespace MVCForum.Data.Repositories
{
    public partial class TopicNotificationRepository : ITopicNotificationRepository
    {
        private readonly MVCForumContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"> </param>
        public TopicNotificationRepository(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        public IList<TopicNotification> GetAll()
        {
            return _context.TopicNotification
                .ToList();
        }

        public IList<TopicNotification> GetByTopic(Topic topic)
        {
            return _context.TopicNotification
                .Where(x => x.Topic.Id == topic.Id)
                .AsNoTracking()
                .ToList();
        }

        public IList<TopicNotification> GetByUser(MembershipUser user)
        {
            return _context.TopicNotification
                .Where(x => x.User.Id == user.Id)
                .ToList();
        }

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

        public TopicNotification Add(TopicNotification topicNotification)
        {
            return _context.TopicNotification.Add(topicNotification);
        }

        public TopicNotification Get(Guid id)
        {
            return _context.TopicNotification.FirstOrDefault(x => x.Id == id);
        }

        public void Delete(TopicNotification item)
        {
            _context.TopicNotification.Remove(item);
        }

    }
}
