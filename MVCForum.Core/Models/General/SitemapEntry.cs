namespace MvcForum.Core.Models.General
{
    using System;
    using Enums;

    public class SitemapEntry
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public DateTime LastUpdated { get; set; }
        public SiteMapChangeFreqency ChangeFrequency { get; set; }
        public string Priority { get; set; }
    }
}
