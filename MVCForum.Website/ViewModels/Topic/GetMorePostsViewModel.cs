namespace MvcForum.Web.ViewModels.Topic
{
    using System;

    public class GetMorePostsViewModel
    {
        public Guid TopicId { get; set; }
        public int PageIndex { get; set; }
        public string Order { get; set; }
    }
}