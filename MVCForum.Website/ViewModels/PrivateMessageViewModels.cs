using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web.Mvc;
using MVCForum.Domain.DomainModel;
using MVCForum.Website.Application;

namespace MVCForum.Website.ViewModels
{
    public class ListPrivateMessageViewModel
    {
        public IList<PrivateMessage> Messages { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    public class CreatePrivateMessageViewModel
    {
        [UIHint(SiteConstants.EditorType), AllowHtml]
        public string Message { get; set; }

        [ForumMvcResourceDisplayName("PM.RecipientUsername")]
        [Required]
        public Guid To { get; set; }

    }

    public class ViewPrivateMessageViewModel
    {
        public IList<PrivateMessage> PrivateMessages { get; set; } 
        public MembershipUser From { get; set; }
    }

    public class DeletePrivateMessageViewModel
    {
        public Guid Id { get; set; }
    }


}