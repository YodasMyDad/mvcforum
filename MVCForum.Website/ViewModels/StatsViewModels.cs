using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MVCForum.Website.ViewModels
{
    public class MainStatsViewModel
    {
        public int PostCount { get; set; }
        public int TopicCount { get; set; }
        public int MemberCount { get; set; }
        public Dictionary<string, string> LatestMembers { get; set; }
    }
}