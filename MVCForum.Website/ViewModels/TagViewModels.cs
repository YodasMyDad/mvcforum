using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Website.ViewModels
{
    public class PopularTagViewModel
    {
        public Dictionary<TopicTag, int> PopularTags { get; set; }
    }
}