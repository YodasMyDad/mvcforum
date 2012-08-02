using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Website.ViewModels
{
    public class CategoryListViewModel
    {
        public Dictionary<Category, PermissionSet> AllPermissionSets { get; set; }
        public Dictionary<Category, IEnumerable<Category>> AllSubCategories { get; set; }
    }

    public class ViewCategoryViewModel
    {
        public PagedList<Topic> Topics { get; set; }
        public PermissionSet Permissions { get; set; }
        public Category Category { get; set; }
        public MembershipUser User { get; set; }
        public bool IsSubscribed { get; set; }

        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
    }
}