using System;

namespace MVCForum.Domain.DomainModel
{
    public enum SiteMapChangeFreqency
    {
        always,
        hourly,
        daily,
        weekly,
        monthly,
        yearly,
        never,
    }

    public class SitemapEntry
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public DateTime LastUpdated { get; set; }
        public SiteMapChangeFreqency ChangeFrequency { get; set; }
        public string Priority { get; set; }
    }
}
