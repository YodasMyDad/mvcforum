namespace MvcForum.Web.ViewModels.Post
{
    using System.Collections.Generic;
    using Core.Models.Entities;
    using Core.Models.General;

    public class ShowMorePostsViewModel
    {
        public List<PostViewModel> Posts { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
        public PermissionSet Permissions { get; set; }
        public Topic Topic { get; set; }
    }
}