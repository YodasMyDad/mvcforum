namespace MvcForum.Web.ViewModels.Topic
{
    using System.Collections.Generic;

    public class ActiveTopicsViewModel
    {
        public List<TopicViewModel> Topics { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
    }
}