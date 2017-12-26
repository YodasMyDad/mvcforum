namespace MvcForum.Web.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.Web.Mvc;
    using Application;
    using Core.Constants;
    using Core.DomainModel.Entities;
    using Core.Interfaces;

    public class ListPrivateMessageViewModel
    {
        public IList<PrivateMessageListItem> Messages { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    public class CreatePrivateMessageViewModel
    {
        [UIHint(AppConstants.EditorType)]
        [AllowHtml]
        public string Message { get; set; }

        [ForumMvcResourceDisplayName("PM.RecipientUsername")]
        [Required]
        public Guid To { get; set; }
    }

    public class ViewPrivateMessageViewModel
    {
        public IPagedList<PrivateMessage> PrivateMessages { get; set; }
        public MembershipUser From { get; set; }
        public bool FromUserIsOnline { get; set; }
        public bool IsAjaxRequest { get; set; }
        public bool IsBlocked { get; set; }
    }

    public class DeletePrivateMessageViewModel
    {
        public Guid Id { get; set; }
    }


    public class GetMoreViewModel
    {
        public Guid UserId { get; set; }
        public int PageIndex { get; set; }
    }
}