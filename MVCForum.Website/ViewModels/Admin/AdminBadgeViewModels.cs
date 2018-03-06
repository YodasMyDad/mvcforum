namespace MvcForum.Web.ViewModels.Admin
{
    using Core.Models.Entities;
    using Core.Models.General;

    public class ListBadgesViewModel
    {
        public PaginatedList<Badge> Badges { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public string Search { get; set; }
    }
}