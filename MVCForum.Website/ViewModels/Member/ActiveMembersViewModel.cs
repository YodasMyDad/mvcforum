namespace MvcForum.Web.ViewModels.Member
{
    using System.Collections.Generic;
    using Core.DomainModel.Entities;

    public class ActiveMembersViewModel
    {
        public IList<MembershipUser> ActiveMembers { get; set; }
    }
}