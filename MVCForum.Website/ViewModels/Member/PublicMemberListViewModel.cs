namespace MvcForum.Web.ViewModels.Member
{
    using System.Collections.Generic;
    using Application;

    public class PublicMemberListViewModel
    {
        [ForumMvcResourceDisplayName("Members.Label.Users")]
        public IList<PublicSingleMemberListViewModel> Users { get; set; }

        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public string Search { get; set; }
    }
}