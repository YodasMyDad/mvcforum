namespace MvcForum.Web.ViewModels.Registration
{
    using System.ComponentModel.DataAnnotations;
    using Application;

    public class LogOnViewModel
    {
        public string ReturnUrl { get; set; }

        [Required]
        [ForumMvcResourceDisplayName("Members.Label.Username")]
        public string UserName { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [ForumMvcResourceDisplayName("Members.Label.Password")]
        public string Password { get; set; }

        [ForumMvcResourceDisplayName("Members.Label.RememberMe")]
        public bool RememberMe { get; set; }
    }
}