namespace MvcForum.Web.ViewModels.Member
{
    using System;
    using Core.DomainModel.Entities;
    using Core.DomainModel.General;

    public class ViewMemberViewModel
    {
        public MembershipUser User { get; set; }
        public Guid LoggedOnUserId { get; set; }
        public PermissionSet Permissions { get; set; }
    }
}