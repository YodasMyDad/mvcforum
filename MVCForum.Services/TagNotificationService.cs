using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Services.Data.Context;

namespace MVCForum.Services
{
    public partial class TagNotificationService : ITagNotificationService
    {
        private readonly MVCForumContext _context;

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="context"> </param>
        public TagNotificationService(IMVCForumContext context)
        {
            _context = context as MVCForumContext;
        }

        public IList<TagNotification> GetAll()
        {
            return _context.TagNotification
                .ToList();
        }

        public void Delete(TagNotification notification)
        {
            _context.TagNotification.Remove(notification);
        }

        public IList<TagNotification> GetByTag(TopicTag tag)
        {
            return _context.TagNotification
                    .Include(x => x.User)
                    .Where(x => x.Tag.Id == tag.Id)
                    .AsNoTracking()
                    .ToList();
        }

        public IList<TagNotification> GetByTag(List<TopicTag> tag)
        {
            var tagIds = tag.Select(x => x.Id);
            return _context.TagNotification
                    .Include(x => x.User)
                    .Where(x => tagIds.Contains(x.Tag.Id))
                    .AsNoTracking()
                    .ToList();
        }

        public IList<TagNotification> GetByUser(MembershipUser user)
        {
            return _context.TagNotification
                .Where(x => x.User.Id == user.Id)
                .ToList();
        }

        public IList<TagNotification> GetByUserAndTag(MembershipUser user, TopicTag tag, bool addTracking = false)
        {
            var notifications = _context.TagNotification
               .Where(x => x.User.Id == user.Id && x.Tag.Id == tag.Id);
            if (addTracking)
            {
                return notifications.ToList();
            }
            return notifications.AsNoTracking().ToList();
        }

        public TagNotification Add(TagNotification tagNotification)
        {
            return _context.TagNotification.Add(tagNotification);
        }

        public TagNotification Get(Guid id)
        {
            return _context.TagNotification.FirstOrDefault(x => x.Id == id);
        }
    }
}
