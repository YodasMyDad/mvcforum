namespace MvcForum.Web.ViewModels.Category
{
    using System.Collections.Generic;
    using Core.Models.Entities;

    public class EditCategoryPermissionsViewModel
    {
        public Category Category { get; set; }
        public List<Permission> Permissions { get; set; }
        public List<MembershipRole> Roles { get; set; }
    }
}