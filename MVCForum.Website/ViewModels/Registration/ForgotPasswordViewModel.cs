namespace MvcForum.Web.ViewModels.Registration
{
    using System.ComponentModel.DataAnnotations;
    using Application;

    public class ForgotPasswordViewModel
    {
        [ForumMvcResourceDisplayName("Members.Label.EmailAddressBlank")]
        [EmailAddress]
        [Required]
        public string EmailAddress { get; set; }
    }
}