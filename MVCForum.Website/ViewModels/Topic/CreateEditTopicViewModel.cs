namespace MvcForum.Web.ViewModels.Topic
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web;
    using System.Web.Mvc;
    using Application;
    using Core.Constants;
    using Core.Models.Entities;

    public class CreateEditTopicViewModel
    {
        public CreateEditTopicViewModel()
        {
            PollAnswers = new List<PollAnswer>();
        }

        [Required]
        [StringLength(100)]
        [ForumMvcResourceDisplayName("Topic.Label.TopicTitle")]
        public string Name { get; set; }

        [UIHint(Constants.EditorType)]
        [AllowHtml]
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

        [ForumMvcResourceDisplayName("Topic.Label.SubscribeToTopic")]
        public bool SubscribeToTopic { get; set; }

        // Permissions stuff
        public CheckCreateTopicPermissions OptionalPermissions { get; set; }

        // Edit Properties
        [HiddenInput]
        public Guid Id { get; set; }

        [HiddenInput]
        public Guid TopicId { get; set; }

        public bool IsTopicStarter { get; set; }

        // Collections

        public List<SelectListItem> Categories { get; set; }

        public List<PollAnswer> PollAnswers { get; set; }

        [ForumMvcResourceDisplayName("Topic.Label.UploadFiles")]
        public HttpPostedFileBase[] Files { get; set; }

        public bool IsPostEdit { get; set; }
    }
}