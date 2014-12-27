using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
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
        public Post StarterPost { get; set; }
        public List<Post> Posts { get; set; }
        public bool ShowCategoryName { get; set; }
        public int VotesUp { get; set; }
        public int VotesDown { get; set; }
        public int Answers { get; set; }
        public int Views { get; set; }
    }

    public class ActiveTopicsViewModel
    {
        public List<TopicViewModel> Topics { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
    }

    public class TagTopicsViewModel
    {
        public List<TopicViewModel> Topics { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
        public string Tag { get; set; }
    }

    public class CreateTopicViewModel
    {
        [Required]
        [StringLength(600)]
        [ForumMvcResourceDisplayName("Topic.Label.TopicTitle")]
        public string Name { get; set; }

        [UIHint(SiteConstants.EditorType), AllowHtml]
        [StringLength(6000)]
        public string Content { get; set; }

        public bool IsSticky { get; set; }
        public bool IsLocked { get; set; }

        [Required]
        [ForumMvcResourceDisplayName("Topic.Label.Category")]
        public Guid Category { get; set; }

        public string Tags { get; set; }

        public IEnumerable<Category> Categories { get; set; }

        public List<PollAnswer> PollAnswers { get; set; }
            
        [ForumMvcResourceDisplayName("Topic.Label.SubscribeToTopic")]
        public bool SubscribeToTopic { get; set; }

        public MembershipUser LoggedOnUser { get; set; }
    }

    public class ShowTopicViewModel
    {
        public Post TopicStarterPost { get; set; }
        public Topic Topic { get; set; }
        public Category Category { get; set; }
        public PagedList<Post> Posts { get; set; }
        public PermissionSet Permissions { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public MembershipUser User { get; set; }
        public bool IsSubscribed { get; set; }
        public bool UserHasAlreadyVotedInPoll { get; set; }
        public int TotalVotesInPoll { get; set; }
        public string PostContent { get; set; }
    }

    public class GetMorePostsViewModel
    {
        public Guid TopicId { get; set; }
        public int PageIndex { get; set; }
        public string Order { get; set; }
    }

    public class ShowPollViewModel
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