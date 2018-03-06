﻿namespace MvcForum.Core.Models.General
{
    using System;

    public partial class RssItem
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public string RssImage { get; set; }
        public DateTime PublishedDate { get; set; }
    }
}
