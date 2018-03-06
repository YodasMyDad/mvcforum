namespace MvcForum.Web.ViewModels.Category
{
    using System.Collections.Generic;
    using Core.Models;
    using Core.Models.Entities;
    using Core.Models.General;

    public class SectionListViewModel
    {
        public Section Section { get; set; }
        public Dictionary<CategorySummary, PermissionSet> AllPermissionSets { get; set; }
    }
}