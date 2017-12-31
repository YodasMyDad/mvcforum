namespace MvcForum.Web.ViewModels.Category
{
    using System.Collections.Generic;
    using Core.Models.Entities;
    using Core.Models.General;

    public class SubCategoryViewModel
    {
        public Dictionary<Category, PermissionSet> AllPermissionSets { get; set; }
        public Category ParentCategory { get; set; }
    }
}