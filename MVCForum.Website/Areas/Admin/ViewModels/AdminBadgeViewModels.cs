using MVCForum.Domain.DomainModel;

namespace MVCForum.Website.Areas.Admin.ViewModels
{
    public class ListBadgesViewModel
    {
        public PagedList<Domain.DomainModel.Badge> Badges { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public string Search { get; set; }
    }

}