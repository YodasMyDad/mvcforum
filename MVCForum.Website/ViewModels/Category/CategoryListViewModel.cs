namespace MvcForum.Web.ViewModels.Category
{
    using System.Collections.Generic;
    using Core.Models;
    using Core.Models.Entities;
    using Core.Models.General;

    public class CategoryListViewModel
    {
        public Dictionary<Category, PermissionSet> AllPermissionSets { get; set; }
    }

    public class CategoryListSummaryViewModel
    {
        public Dictionary<CategorySummary, PermissionSet> AllPermissionSets { get; set; }
    }
}