namespace MvcForum.Web.ViewModels.PrivateMessage
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;
    using Application;
    using Core.Constants;

    public class CreatePrivateMessageViewModel
    {
        [UIHint(Constants.EditorType)]
        [AllowHtml]
        public string Message { get; set; }

        [ForumMvcResourceDisplayName("PM.RecipientUsername")]
        [Required]
        public Guid To { get; set; }
    }
}