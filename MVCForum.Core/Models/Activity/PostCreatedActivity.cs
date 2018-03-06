namespace MvcForum.Core.Models.Activity
{
    using System;
    using Entities;
    using Enums;

    public class PostCreatedActivity : ActivityBase
    {
        public Post Post { get; set; }

        public PostCreatedActivity(Activity activity, Post post)
        {
            ActivityMapped = activity;
            Post = post;
        }

        public static Activity GenerateMappedRecord(Post post, DateTime timestamp)
        {
            return new Activity
            {
                Data = post.Id.ToString(),
                Timestamp = timestamp,
                Type = ActivityType.PostCreated.ToString()
            };
        }
    }
}