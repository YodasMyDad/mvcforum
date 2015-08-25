using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Repositories
{
    public partial interface ITagNotificationRepository
    {
        IList<TagNotification> GetAll();
        void Delete(TagNotification notification);
        IList<TagNotification> GetByTag(TopicTag tag);
        IList<TagNotification> GetByUser(MembershipUser user);
        IList<TagNotification> GetByUserAndTag(MembershipUser user, TopicTag tag, bool addTracking = false);
        TagNotification Add(TagNotification tagNotification);
        TagNotification Get(Guid id);
    }
}
