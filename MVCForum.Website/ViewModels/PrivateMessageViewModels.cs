using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using MVCForum.Domain.Constants;
using MVCForum.Domain.DomainModel;
using MVCForum.Website.Application;

namespace MVCForum.Website.ViewModels
{
    public class ListPrivateMessageViewModel
    {
        public IList<PrivateMessage> Messages { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
    }

    public class CreatePrivateMessageViewModel
    {
        [ForumMvcResourceDisplayName("PM.RecipientUsername")]
        [StringLength(150)]
        [Required]
        public string UserToUsername { get; set; }

        [ForumMvcResourceDisplayName("PM.MessageSubject")]
        [Required]
        public string Subject { get; set; }

        [UIHint(AppConstants.EditorType), AllowHtml]
        public string Message { get; set; }

        public string PreviousMessage { get; set; }

    }

    public class ViewPrivateMessageViewModel
    {
        public PrivateMessage Message { get; set; }
    }

    public class DeletePrivateMessageViewModel
    {
        public Guid Id { get; set; }
    }


}