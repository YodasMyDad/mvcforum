namespace MvcForum.Web.ViewModels.Category
{
    using System.Collections.Generic;
    using Core.DomainModel.Entities;
    using Core.DomainModel.General;

    public class CategoryListViewModel
    {
        public Dictionary<Category, PermissionSet> AllPermissionSets { get; set; }
    }
}