namespace MvcForum.Web.ViewModels.Breadcrumb
{
    using System.Collections.Generic;
    using Core.Models.Entities;

    public class BreadcrumbViewModel
    {
        public List<Category> Categories { get; set; }
        public Topic Topic { get; set; }
        public Category Category { get; set; }
    }
}