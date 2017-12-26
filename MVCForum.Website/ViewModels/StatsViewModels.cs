namespace MvcForum.Web.ViewModels
{
    using System.Collections.Generic;
    using Core.DomainModel.Entities;

    public class MainStatsViewModel
    {
        public int PostCount { get; set; }
        public int TopicCount { get; set; }
        public int MemberCount { get; set; }
        public IList<MembershipUser> LatestMembers { get; set; }
    }
}