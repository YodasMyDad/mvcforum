namespace MvcForum.Plugins.Pipelines.Topic
{
    using System;
    using System.Threading.Tasks;
    using Core.Interfaces;
    using Core.Interfaces.Pipeline;
    using Core.Models.Entities;

    public class TopicNotificationsAndBadgesPipe : IPipe<IPipelineProcess<Topic>>
    {
        /// <inheritdoc />
        public async Task<IPipelineProcess<Topic>> Process(IPipelineProcess<Topic> input, IMvcForumContext context)
        {
            // If tags
            //_badgeService.ProcessBadge(BadgeType.Tag, topic.User);

            //// Subscribe the user to the topic as they have checked the checkbox
            //if (topicViewModel.SubscribeToTopic)
            //{
            //    // Create the notification
            //    var topicNotification = new TopicNotification
            //    {
            //        Topic = topic,
            //        User = loggedOnUser
            //    };
            //    //save
            //    _topicNotificationService.Add(topicNotification);
            //}


            //if (!topic.Pending.HasValue || !topic.Pending.Value)
            //{
            //    _activityService.TopicCreated(topic);
            //}


            //if (successfullyCreated && !cancelledByEvent)
            //{
            //    // Success so now send the emails
            //    NotifyNewTopics(category, topic, loggedOnReadOnlyUser);

            //    // Redirect to the newly created topic
            //    return Redirect($"{topic.NiceUrl}?postbadges=true");
            //}

            throw new NotImplementedException();
        }
    }
}