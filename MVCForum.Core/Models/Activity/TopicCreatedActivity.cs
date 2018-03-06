namespace MvcForum.Core.Models.Activity
{
    using System;
    using Entities;
    using Enums;

    public class TopicCreatedActivity : ActivityBase
    {
        public Topic Topic { get; set; }

        public TopicCreatedActivity(Activity activity, Topic topic)
        {
            ActivityMapped = activity;
            Topic = topic;
        }

        public static Activity GenerateMappedRecord(Topic topic, DateTime timestamp)
        {
            return new Activity
            {
                Data = topic.Id.ToString(),
                Timestamp = timestamp,
                Type = ActivityType.TopicCreated.ToString()
            };

        }
    }
}