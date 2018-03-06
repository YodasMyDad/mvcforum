namespace MvcForum.Web.ViewModels.Topic
{
    using System;
    using System.Collections.Generic;
    using Core.Models.Entities;
    using Core.Models.General;
    using Poll;
    using Post;

    public class TopicViewModel
    {
        public Topic Topic { get; set; }
        public PermissionSet Permissions { get; set; }
        public bool MemberIsOnline { get; set; }

        // Poll
        public PollViewModel Poll { get; set; }

        // Post Stuff
        public PostViewModel StarterPost { get; set; }

        public List<PostViewModel> Posts { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
        public string LastPostPermaLink { get; set; }

        // Permissions
        public bool DisablePosting { get; set; }

        // Subscription
        public bool IsSubscribed { get; set; }

        // Votes
        public int VotesUp { get; set; }

        public int VotesDown { get; set; }

        // Quote/Reply
        public string QuotedPost { get; set; }

        public Guid? ReplyTo { get; set; }
        public string ReplyToUsername { get; set; }

        // Stats
        public int Answers { get; set; }

        public int Views { get; set; }

        // Misc
        public bool ShowUnSubscribedLink { get; set; }
    }
}