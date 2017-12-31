namespace MvcForum.Web.ViewModels.Tag
{
    using System.Collections.Generic;
    using Core.Models.Entities;

    public class PopularTagViewModel
    {
        public Dictionary<TopicTag, int> PopularTags { get; set; }
    }
}