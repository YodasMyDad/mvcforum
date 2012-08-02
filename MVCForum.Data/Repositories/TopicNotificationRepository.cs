using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using MVCForum.Data.Context;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;


namespace MVCForum.Data.Repositories
{
    public class TopicNotificationRepository : ITopicNotificationRepository
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
                .ToList();
        }

        public IList<TopicNotification> GetByUser(MembershipUser user)
        {
            return _context.TopicNotification
                .Where(x => x.User.Id == user.Id)
                .ToList();
        }

        public IList<TopicNotification> GetByUserAndTopic(MembershipUser user, Topic topic)
        {
            return _context.TopicNotification
                .Where(x => x.User.Id == user.Id && x.Topic.Id == topic.Id)
                .ToList();
        }

        public TopicNotification Add(TopicNotification topicNotification)
        {
            topicNotification.Id = Guid.NewGuid();
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

        public void Update(TopicNotification item)
        {
            // Check there's not an object with same identifier already in context
            if (_context.TopicNotification.Local.Select(x => x.Id == item.Id).Any())
            {
                throw new ApplicationException("Object already exists in context - you do not need to call Update. Save occurs on Commit");
            }
            _context.Entry(item).State = EntityState.Modified;  
        }
    }
}
