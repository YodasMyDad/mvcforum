namespace MvcForum.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using ExtensionMethods;
    using Interfaces;
    using Interfaces.Services;
    using Models;
    using Models.Entities;
    using Models.Enums;
    using Utilities;

    public partial class NotificationService : INotificationService
    {
        private IMvcForumContext _context;
        private readonly ILocalizationService _localizationService;
        private readonly IEmailService _emailService;
        private readonly ISettingsService _settingsService;
        private readonly ILoggingService _loggingService;

        public NotificationService(IMvcForumContext context, ILocalizationService localizationService, IEmailService emailService, ISettingsService settingsService, ILoggingService loggingService)
        {
            _context = context;
            _localizationService = localizationService;
            _emailService = emailService;
            _settingsService = settingsService;
            _loggingService = loggingService;
        }

        /// <inheritdoc />
        public void RefreshContext(IMvcForumContext context)
        {
            _context = context;
            _localizationService.RefreshContext(context);
            _emailService.RefreshContext(context);
            _settingsService.RefreshContext(context);
        }

        /// <inheritdoc />
        public async Task<int> SaveChanges()
        {
            return await _context.SaveChangesAsync();
        }

        #region Category Notifications

        /// <inheritdoc />
        public void Delete(CategoryNotification notification)
        {
            _context.CategoryNotification.Remove(notification);
        }

        /// <inheritdoc />
        public List<CategoryNotification> GetCategoryNotificationsByCategory(Category category)
        {
            return _context.CategoryNotification.AsNoTracking()
                .Where(x => x.Category.Id == category.Id)
                .ToList();
        }

        /// <inheritdoc />
        public List<CategoryNotification> GetCategoryNotificationsByUser(MembershipUser user)
        {
            return _context.CategoryNotification.AsNoTracking()
                .Where(x => x.User.Id == user.Id)
                .ToList();
        }

        /// <inheritdoc />
        public List<CategoryNotification> GetCategoryNotificationsByUserAndCategory(MembershipUser user,
            Category category, bool addTracking = false)
        {
            var notifications = _context.CategoryNotification.Where(x => x.Category.Id == category.Id && x.User.Id == user.Id);
            if (addTracking)
            {
                return notifications.ToList();
            }
            return notifications.AsNoTracking().ToList();
        }

        /// <inheritdoc />
        public CategoryNotification Add(CategoryNotification category)
        {
            return _context.CategoryNotification.Add(category);
        }

        #endregion

        #region Tag Notification

        /// <inheritdoc />
        public void Delete(TagNotification notification)
        {
            _context.TagNotification.Remove(notification);
        }

        /// <inheritdoc />
        public IList<TagNotification> GetTagNotificationsByTag(TopicTag tag)
        {
            return _context.TagNotification
                .Include(x => x.User)
                .Where(x => x.Tag.Id == tag.Id)
                .AsNoTracking()
                .ToList();
        }

        /// <inheritdoc />
        public IList<TagNotification> GetTagNotificationsByTag(List<TopicTag> tag)
        {
            var tagIds = tag.Select(x => x.Id);
            return _context.TagNotification
                .Include(x => x.User)
                .Where(x => tagIds.Contains(x.Tag.Id))
                .AsNoTracking()
                .ToList();
        }

        /// <inheritdoc />
        public IList<TagNotification> GetTagNotificationsByUser(MembershipUser user)
        {
            return _context.TagNotification
                .Where(x => x.User.Id == user.Id)
                .ToList();
        }

        /// <inheritdoc />
        public IList<TagNotification> GetTagNotificationsByUserAndTag(MembershipUser user, TopicTag tag,
            bool addTracking = false)
        {
            var notifications = _context.TagNotification
                .Where(x => x.User.Id == user.Id && x.Tag.Id == tag.Id);
            if (addTracking)
            {
                return notifications.ToList();
            }
            return notifications.AsNoTracking().ToList();
        }

        /// <inheritdoc />
        public TagNotification Add(TagNotification tagNotification)
        {
            return _context.TagNotification.Add(tagNotification);
        }

        #endregion

        #region Topic Notification

        /// <inheritdoc />
        public void Delete(TopicNotification notification)
        {
            _context.TopicNotification.Remove(notification);
        }

        /// <inheritdoc />
        public IList<TopicNotification> GetTopicNotificationsByTopic(Topic topic)
        {
            return _context.TopicNotification
                .Where(x => x.Topic.Id == topic.Id)
                .AsNoTracking()
                .ToList();
        }

        /// <inheritdoc />
        public IList<TopicNotification> GetTopicNotificationsByUser(MembershipUser user)
        {
            return _context.TopicNotification
                .Where(x => x.User.Id == user.Id)
                .ToList();
        }

        /// <inheritdoc />
        public IList<TopicNotification> GetTopicNotificationsByUserAndTopic(MembershipUser user, Topic topic,
            bool addTracking = false)
        {
            var notifications = _context.TopicNotification
                .Where(x => x.User.Id == user.Id && x.Topic.Id == topic.Id);
            if (addTracking)
            {
                return notifications.ToList();
            }
            return notifications.AsNoTracking().ToList();
        }

        /// <inheritdoc />
        public TopicNotification Add(TopicNotification topicNotification)
        {
            return _context.TopicNotification.Add(topicNotification);
        }

        /// <inheritdoc />
        public void Notify(Topic topic, MembershipUser loggedOnReadOnlyUser, NotificationType notificationType)
        {
            List<Guid> userIdsToNotify;

            var settings = _settingsService.GetSettings();

            if (notificationType == NotificationType.Post)
            {
                userIdsToNotify = GetTopicNotificationsByTopic(topic).Select(x => x.User.Id).ToList();
            }
            else
            {
                // Get all notifications for this category and for the tags on the topic
                userIdsToNotify = GetCategoryNotificationsByCategory(topic.Category).Select(x => x.User.Id).ToList();

                // Merge and remove duplicate ids
                if (topic.Tags != null && topic.Tags.Any())
                {
                    var tagNotifications = GetTagNotificationsByTag(topic.Tags.ToList()).Select(x => x.User.Id)
                        .ToList();
                    userIdsToNotify = userIdsToNotify.Union(tagNotifications).ToList();
                }
            }

            if (userIdsToNotify.Any())
            {
                // remove the current user from the notification, don't want to notify yourself that you 
                // have just made a topic!
                userIdsToNotify.Remove(loggedOnReadOnlyUser.Id);

                if (userIdsToNotify.Count > 0)
                {
                    try
                    {
                        // Now get all the users that need notifying
                        var users = _context.MembershipUser
                            .Where(x => userIdsToNotify.Contains(x.Id))
                            .AsNoTracking()
                            .ToList();

                        // Create the email
                        var sb = new StringBuilder();
                        sb.AppendFormat("<p>{0}</p>",
                            string.Format(_localizationService.GetResourceString(notificationType == NotificationType.Post ? "Post.Notification.NewPosts" : "Topic.Notification.NewTopics"), 
                            topic.Category.Name));
                        sb.Append($"<p>{topic.Name}</p>");
                        if (ForumConfiguration.Instance.IncludeFullPostInEmailNotifications)
                        {
                            sb.Append(topic.LastPost.PostContent.ConvertPostContent());
                        }
                        sb.AppendFormat("<p><a href=\"{0}\">{0}</a></p>", string.Concat(StringUtils.ReturnCurrentDomain(), topic.Category.NiceUrl));

                        // create the emails and only send them to people who have not had notifications disabled
                        var emails = users
                            .Where(x => x.DisableEmailNotifications != true && !x.IsLockedOut && x.IsBanned != true).Select(
                                user => new Email
                                {
                                    Body = _emailService.EmailTemplate(user.UserName, sb.ToString()),
                                    EmailTo = user.Email,
                                    NameTo = user.UserName,
                                    Subject = string.Concat(
                                        _localizationService.GetResourceString(notificationType == NotificationType.Post ? "Post.Notification.Subject" : "Topic.Notification.Subject"),
                                        settings.ForumName)
                                }).ToList();

                        // and now pass the emails in to be sent
                        _emailService.SendMail(emails);

                        _context.SaveChanges();
                    }
                    catch (Exception ex)
                    {
                        _context.RollBack();
                        _loggingService.Error(ex);
                    }
                }
            }
        }

        #endregion
    }
}