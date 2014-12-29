using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using MVCForum.Domain.DomainModel;
using MVCForum.Website.Application;

namespace MVCForum.Website.ViewModels
{
    public class CreateAjaxPostViewModel
    {
        [UIHint(SiteConstants.EditorType), AllowHtml]
        [StringLength(6000)]
        public string PostContent { get; set; }
        public Guid Topic { get; set; }
        public bool DisablePosting { get; set; }
    }

    public class ShowMorePostsViewModel
    {
        public PagedList<Post> Posts { get; set; }
        public PermissionSet Permissions { get; set; }
        public MembershipUser User { get; set; }
        public Topic Topic { get; set; }
    }

    public class PostViewModel
    {
        public Post Post { get; set; }
        public Topic ParentTopic { get; set; }
        public PermissionSet Permissions { get; set; }
        public MembershipUser User { get; set; }
    }

    public class EditPostViewModel
    {
        [ForumMvcResourceDisplayName("Post.Label.TopicName")]
        [Required]
        [StringLength(600)]
        public string Name { get; set; }

        [ForumMvcResourceDisplayName("Post.Label.IsStickyTopic")]
        public bool IsSticky { get; set; }

        [ForumMvcResourceDisplayName("Post.Label.LockTopic")]
        public bool IsLocked { get; set; }

        [Required]
        [ForumMvcResourceDisplayName("Post.label.TopicCategory")]
        public Guid Category { get; set; }

        public string Tags { get; set; }

        public IList<PollAnswer> PollAnswers { get; set; }

        public IEnumerable<Category> Categories { get; set; }

        [UIHint(SiteConstants.EditorType), AllowHtml]
        [StringLength(6000)]
        public string Content { get; set; }

        [HiddenInput]
        public Guid Id { get; set; }

        public bool IsTopicStarter { get; set; }

        public PermissionSet Permissions { get; set; }
    }

    public class ReportPostViewModel
    {
        public Guid PostId { get; set; }
        public string PostCreatorUsername { get; set; }
        
        [Required]
        public string Reason { get; set; }
    }
}