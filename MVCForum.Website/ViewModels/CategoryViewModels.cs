using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Website.ViewModels
{
    public class BreadcrumbViewModel
    {
        public List<Category> Categories { get; set; }
        public Topic Topic { get; set; }
        public Category Category { get; set; }
    }

    public class CategoryListViewModel
    {
        public Dictionary<Category, PermissionSet> AllPermissionSets { get; set; }
    }


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

    public class SubCategoryViewModel
    {
        public Dictionary<Category, PermissionSet> AllPermissionSets { get; set; }
        public Category ParentCategory { get; set; }
    }

}