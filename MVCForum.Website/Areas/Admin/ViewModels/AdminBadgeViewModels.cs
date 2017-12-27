namespace MvcForum.Web.Areas.Admin.ViewModels
{
    using Core.DomainModel.Entities;
    using Core.DomainModel.General;
    using Core.Models.General;

    public class ListBadgesViewModel
    {
        public PaginatedList<Badge> Badges { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public string Search { get; set; }
    }
}