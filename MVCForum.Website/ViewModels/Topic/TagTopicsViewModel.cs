namespace MvcForum.Web.ViewModels.Topic
{
    using System;
    using System.Collections.Generic;

    public class TagTopicsViewModel
    {
        public List<TopicViewModel> Topics { get; set; }
        public int? PageIndex { get; set; }
        public int? TotalCount { get; set; }
        public int? TotalPages { get; set; }
        public string Tag { get; set; }
        public Guid TagId { get; set; }
        public bool IsSubscribed { get; set; }
    }
}