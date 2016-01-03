using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.DomainModel.Entities;
using MVCForum.Website.Application;

namespace MVCForum.Website.ViewModels
{
    public class CreateAjaxPostViewModel
    {
        [UIHint(AppConstants.EditorType), AllowHtml]
        [StringLength(6000)]
        public string PostContent { get; set; }
        public Guid Topic { get; set; }
        public bool DisablePosting { get; set; }
        public Guid? InReplyTo { get; set; }
        public string ReplyToUsername { get; set; }
    }

    public class ShowMorePostsViewModel
    {
        public List<PostViewModel> Posts { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
        public PermissionSet Permissions { get; set; }
        public Topic Topic { get; set; }
    }

    public class PostLikedByViewModel
    {
        public List<Vote> Votes { get; set; }
        public Post Post { get; set; }
    }

    public class PostViewModel
    {
        public Post Post { get; set; }
        public string PermaLink { get; set; }
        public List<Vote> Votes { get; set; }
        public List<Favourite> Favourites { get; set; } 
        public Topic ParentTopic { get; set; }
        public PermissionSet Permissions { get; set; }
        public bool AllowedToVote { get; set; }
        public bool HasVotedUp { get; set; }
        public bool HasVotedDown { get; set; }
        public bool MemberHasFavourited { get; set; }
        public bool MemberIsOnline { get; set; }
        public bool ShowTopicName { get; set; }
        public bool MinimalPost { get; set; }
    }

    public class ReportPostViewModel
    {
        public Guid PostId { get; set; }
        public string PostCreatorUsername { get; set; }
        
        [Required]
        public string Reason { get; set; }
    }

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

    public class PostEditHistoryViewModel
    {
        public IList<PostEdit> PostEdits { get; set; } 
    }
}