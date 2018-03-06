namespace MvcForum.Web.ViewModels.Member
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Web;
    using System.Web.Mvc;
    using Application;
    using Core.Constants;

    public class MemberFrontEndEditViewModel
    {
        [Required]
        public Guid Id { get; set; }

        [Required]
        [ForumMvcResourceDisplayName("Members.Label.Username")]
        [StringLength(150, MinimumLength = 4)]
        public string UserName { get; set; }

        [ForumMvcResourceDisplayName("Members.Label.EmailAddress")]
        [EmailAddress]
        [Required]
        public string Email { get; set; }

        [ForumMvcResourceDisplayName("Members.Label.Signature")]
        [StringLength(1000)]
        [UIHint(Constants.EditorType)]
        [AllowHtml]
        public string Signature { get; set; }

        [ForumMvcResourceDisplayName("Members.Label.Age")]
        [Range(0, int.MaxValue)]
        public int? Age { get; set; }

        [ForumMvcResourceDisplayName("Members.Label.Location")]
        [StringLength(100)]
        public string Location { get; set; }

        [ForumMvcResourceDisplayName("Members.Label.Website")]
        [Url]
        [StringLength(100)]
        public string Website { get; set; }

        [ForumMvcResourceDisplayName("Members.Label.Twitter")]
        [Url]
        [StringLength(60)]
        public string Twitter { get; set; }

        [ForumMvcResourceDisplayName("Members.Label.UploadNewAvatar")]
        public HttpPostedFileBase[] Files { get; set; }

        [ForumMvcResourceDisplayName("Members.Label.Facebook")]
        [Url]
        [StringLength(60)]
        public string Facebook { get; set; }

        public string Avatar { get; set; }
        public bool DisableFileUploads { get; set; }

        [ForumMvcResourceDisplayName("Members.Label.DisableEmailNotifications")]
        public bool DisableEmailNotifications { get; set; }


        public int AmountOfPoints { get; set; }
    }
}