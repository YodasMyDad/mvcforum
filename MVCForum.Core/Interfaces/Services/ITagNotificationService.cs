using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public partial interface ITagNotificationService
    {
        IList<TagNotification> GetAll();
        void Delete(TagNotification notification);
        IList<TagNotification> GetByTag(TopicTag tag);
        IList<TagNotification> GetByTag(List<TopicTag> tag);
        IList<TagNotification> GetByUser(MembershipUser user);
        IList<TagNotification> GetByUserAndTag(MembershipUser user, TopicTag tag, bool addTracking = false);
        void Add(TagNotification tagNotification);
    }
}
