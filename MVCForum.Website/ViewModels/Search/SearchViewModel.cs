namespace MvcForum.Web.ViewModels.Search
{
    using System.Collections.Generic;
    using Post;

    public class SearchViewModel
    {
        public List<PostViewModel> Posts { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
        public string Term { get; set; }
    }
}