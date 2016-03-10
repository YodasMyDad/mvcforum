using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Website.Application;

namespace MVCForum.Website.ViewModels
{
    public class CreateTopicButtonViewModel
    {
        public MembershipUser LoggedOnUser { get; set; }
        public bool UserCanPostTopics { get; set; }
    }

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

    public class ActiveTopicsViewModel
    {
        public List<TopicViewModel> Topics { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
    }

    public class PostedInViewModel
    {
        public List<TopicViewModel> Topics { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
    }

    public class HotTopicsViewModel
    {
        public List<TopicViewModel> Topics { get; set; }
    }

    public class TagTopicsViewModel
    {
        public List<TopicViewModel> Topics { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
        public string Tag { get; set; }
        public Guid TagId { get; set; }
        public bool IsSubscribed { get; set; }
    }

    public class CheckCreateTopicPermissions
    {
        public bool CanUploadFiles { get; set; }
        public bool CanStickyTopic { get; set; }
        public bool CanLockTopic { get; set; }
        public bool CanCreatePolls { get; set; }
        public bool CanInsertImages { get; set; }
    }

    public class CreateEditTopicViewModel
    {
        [Required]
        [StringLength(100)]
        [ForumMvcResourceDisplayName("Topic.Label.TopicTitle")]
        public string Name { get; set; }

        [UIHint(AppConstants.EditorType), AllowHtml]
        [StringLength(6000)]
        public string Content { get; set; }

        [ForumMvcResourceDisplayName("Post.Label.IsStickyTopic")]
        public bool IsSticky { get; set; }

        [ForumMvcResourceDisplayName("Post.Label.LockTopic")]
        public bool IsLocked { get; set; }

        [Required]
        [ForumMvcResourceDisplayName("Topic.Label.Category")]
        public Guid Category { get; set; }

        public string Tags { get; set; }

        [ForumMvcResourceDisplayName("Topic.Label.PollCloseAfterDays")]
        public int PollCloseAfterDays { get; set; }

        public List<SelectListItem> Categories { get; set; }

        public IList<PollAnswer> PollAnswers { get; set; }
            
        [ForumMvcResourceDisplayName("Topic.Label.SubscribeToTopic")]
        public bool SubscribeToTopic { get; set; }

        [ForumMvcResourceDisplayName("Topic.Label.UploadFiles")]
        public HttpPostedFileBase[] Files { get; set; }

        // Permissions stuff
        public CheckCreateTopicPermissions OptionalPermissions { get; set; }

        // Edit Properties
        [HiddenInput]
        public Guid Id { get; set; }

        [HiddenInput]
        public Guid TopicId { get; set; }

        public bool IsTopicStarter { get; set; }
    }

    public class GetMorePostsViewModel
    {
        public Guid TopicId { get; set; }
        public int PageIndex { get; set; }
        public string Order { get; set; }
    }

    public class PollViewModel
    {
        public Poll Poll { get; set; }
        public bool UserHasAlreadyVoted { get; set; }
        public int TotalVotesInPoll { get; set; }
        public bool UserAllowedToVote { get; set; }
    }

    public class UpdatePollViewModel
    {
        public Guid PollId { get; set; }
        public Guid AnswerId { get; set; }
    }

    public class MoveTopicViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public Guid CategoryId { get; set; }
        public List<Category> Categories { get; set; }
    }

    public class NotifyNewTopicViewModel
    {
        public Guid CategoryId { get; set; }
    }
}