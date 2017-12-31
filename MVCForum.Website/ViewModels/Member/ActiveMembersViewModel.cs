namespace MvcForum.Web.ViewModels.Member
{
    using System.Collections.Generic;
    using Core.Models.Entities;

    public class ActiveMembersViewModel
    {
        public IList<MembershipUser> ActiveMembers { get; set; }
    }
}