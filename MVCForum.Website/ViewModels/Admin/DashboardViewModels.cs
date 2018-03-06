namespace MvcForum.Web.ViewModels.Admin
{
    using System.Collections.Generic;
    using Core.Models.Entities;

    public class MainDashboardNavViewModel
    {
        public int PrivateMessageCount { get; set; }
        public int ModerateCount { get; set; }
    }

    public class LatestUsersViewModels
    {
        public IList<MembershipUser> Users { get; set; }
    }

    public class LowestPointUsersViewModels
    {
        public Dictionary<MembershipUser, int> Users { get; set; }
    }

    public class LowestPointPostsViewModels
    {
        public IList<Post> Posts { get; set; }
    }

    public class HighestViewedTopics
    {
        public IList<Topic> Topics { get; set; }
    }

    public class LatestNewsViewModel
    {
        public IList<MvcForum.Core.Utilities.RssItem> RssFeed { get; set; }
    }

    public class TodaysTopics
    {
        public IList<Topic> Topics { get; set; }
    }
}