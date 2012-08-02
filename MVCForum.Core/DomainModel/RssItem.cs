using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MVCForum.Domain.Interfaces;

namespace MVCForum.Domain.DomainModel
{
    public class RssItem : Entity
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Link { get; set; }
        public DateTime PublishedDate { get; set; }
    }
}
