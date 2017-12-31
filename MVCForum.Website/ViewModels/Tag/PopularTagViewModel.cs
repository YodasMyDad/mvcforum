namespace MvcForum.Web.ViewModels.Tag
{
    using System.Collections.Generic;
    using Core.DomainModel.Entities;

    public class PopularTagViewModel
    {
        public Dictionary<TopicTag, int> PopularTags { get; set; }
    }
}