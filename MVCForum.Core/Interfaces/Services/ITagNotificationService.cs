namespace MvcForum.Core.Interfaces.Services
{
    using System;
    using System.Collections.Generic;
    using DomainModel.Entities;

    public partial interface ITagNotificationService
    {
        IList<TagNotification> GetAll();
        void Delete(TagNotification notification);
        IList<TagNotification> GetByTag(TopicTag tag);
        IList<TagNotification> GetByTag(List<TopicTag> tag);
        IList<TagNotification> GetByUser(MembershipUser user);
        IList<TagNotification> GetByUserAndTag(MembershipUser user, TopicTag tag, bool addTracking = false);
        TagNotification Add(TagNotification tagNotification);
        TagNotification Get(Guid id);
    }
}