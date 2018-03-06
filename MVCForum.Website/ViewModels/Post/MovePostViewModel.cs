namespace MvcForum.Web.ViewModels.Post
{
    using System;
    using System.Collections.Generic;
    using System.Web.Mvc;
    using Application;

    public class MovePostViewModel
    {
        public PostViewModel Post { get; set; }

        public IList<SelectListItem> LatestTopics { get; set; }

        [ForumMvcResourceDisplayName("Post.Move.Label.Topic")]
        public Guid? TopicId { get; set; }

        public Guid PostId { get; set; }

        [ForumMvcResourceDisplayName("Post.Move.Label.NewTopicTitle")]
        public string TopicTitle { get; set; }

        [ForumMvcResourceDisplayName("Post.Move.Label.ReplyToPosts")]
        public bool MoveReplyToPosts { get; set; }
    }
}