using System;

namespace MVCForum.Domain.DomainModel
{
    public class SitemapEntry
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public DateTime LastUpdated { get; set; }
    }
}
