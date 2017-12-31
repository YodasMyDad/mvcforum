namespace MvcForum.Web.ViewModels.Registration
{
    using System.ComponentModel.DataAnnotations;
    using Application;

    public class ChangePasswordViewModel
    {
        [Required]
        [DataType(DataType.Password)]
        [ForumMvcResourceDisplayName("Members.Label.CurrentPassword")]
        public string OldPassword { get; set; }

        [Required]
        [StringLength(100, MinimumLength = 6)]
        [DataType(DataType.Password)]
        [ForumMvcResourceDisplayName("Members.Label.NewPassword")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [ForumMvcResourceDisplayName("Members.Label.ConfirmNewPassword")]
        [Compare("NewPassword")]
        public string ConfirmPassword { get; set; }
    }
}