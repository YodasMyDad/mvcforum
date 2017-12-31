namespace MvcForum.Web.ViewModels.Category
{
    using System.Collections.Generic;
    using Core.Models.Entities;
    using Core.Models.General;
    using Topic;

    public class CategoryViewModel
    {
        public List<TopicViewModel> Topics { get; set; }
        public PermissionSet Permissions { get; set; }
        public Category Category { get; set; }
        public CategoryListViewModel SubCategories { get; set; }
        public MembershipUser User { get; set; }
        public bool IsSubscribed { get; set; }

        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
        public int PostCount { get; set; }

        // Topic info
        public Topic LatestTopic { get; set; }

        public int TopicCount { get; set; }

        // Misc
        public bool ShowUnSubscribedLink { get; set; }
    }


}