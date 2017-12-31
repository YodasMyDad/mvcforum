namespace MvcForum.Web.ViewModels.Stats
{
    using System.Collections.Generic;
    using Core.Models.Entities;

    public class MainStatsViewModel
    {
        public int PostCount { get; set; }
        public int TopicCount { get; set; }
        public int MemberCount { get; set; }
        public IList<MembershipUser> LatestMembers { get; set; }
    }
}