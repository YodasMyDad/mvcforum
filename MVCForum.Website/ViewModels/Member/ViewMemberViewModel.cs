namespace MvcForum.Web.ViewModels.Member
{
    using System;
    using Core.Models.Entities;
    using Core.Models.General;

    public class ViewMemberViewModel
    {
        public MembershipUser User { get; set; }
        public Guid LoggedOnUserId { get; set; }
        public PermissionSet Permissions { get; set; }
    }
}