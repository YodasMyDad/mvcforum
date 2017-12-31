namespace MvcForum.Web.ViewModels.Home
{
    using Core.Models.Activity;
    using Core.Models.General;

    public class AllRecentActivitiesViewModel
    {
        public PaginatedList<ActivityBase> Activities { get; set; }

        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
    }
}