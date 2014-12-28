using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Website.ViewModels
{
    public class MainStatsViewModel
    {
        public int PostCount { get; set; }
        public int TopicCount { get; set; }
        public int MemberCount { get; set; }
        public IList<MembershipUser> LatestMembers { get; set; }
    }
}