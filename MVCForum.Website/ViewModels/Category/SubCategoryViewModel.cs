namespace MvcForum.Web.ViewModels.Category
{
    using System.Collections.Generic;
    using Core.DomainModel.Entities;
    using Core.DomainModel.General;

    public class SubCategoryViewModel
    {
        public Dictionary<Category, PermissionSet> AllPermissionSets { get; set; }
        public Category ParentCategory { get; set; }
    }
}