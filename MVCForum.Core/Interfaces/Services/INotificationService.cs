namespace MvcForum.Core.Interfaces.Services
{
    using System.Collections.Generic;
    using Models.Entities;
    using Models.Enums;

    public partial interface INotificationService : IContextService
    {
        #region Categories

        void Delete(CategoryNotification notification);
        List<CategoryNotification> GetCategoryNotificationsByCategory(Category category);
        List<CategoryNotification> GetCategoryNotificationsByUser(MembershipUser user);
        List<CategoryNotification> GetCategoryNotificationsByUserAndCategory(MembershipUser user, Category category, bool addTracking = false);
        CategoryNotification Add(CategoryNotification category);

        #endregion

        #region Tags

        void Delete(TagNotification notification);
        IList<TagNotification> GetTagNotificationsByTag(TopicTag tag);
        IList<TagNotification> GetTagNotificationsByTag(List<TopicTag> tag);
        IList<TagNotification> GetTagNotificationsByUser(MembershipUser user);
        IList<TagNotification> GetTagNotificationsByUserAndTag(MembershipUser user, TopicTag tag, bool addTracking = false);
        TagNotification Add(TagNotification tagNotification);

        #endregion

        #region Topic

        void Delete(TopicNotification notification);
        IList<TopicNotification> GetTopicNotificationsByTopic(Topic topic);
        IList<TopicNotification> GetTopicNotificationsByUser(MembershipUser user);
        IList<TopicNotification> GetTopicNotificationsByUserAndTopic(MembershipUser user, Topic topic, bool addTracking = false);
        TopicNotification Add(TopicNotification topicNotification);
        #endregion

        void Notify(Topic topic, MembershipUser loggedOnReadOnlyUser, NotificationType notificationType);
    }
}