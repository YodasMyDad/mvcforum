using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;
using System.Xml.Linq;

namespace MVCForum.Utilities
{
    public class RssReader
    {
        public List<RssItem> GetRssFeed(string url)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method = "GET";
            req.UserAgent = "Fiddler";

            var rep = req.GetResponse();
            var reader = XmlReader.Create(rep.GetResponseStream());
            var doc = XDocument.Load(reader, LoadOptions.None);

            return (from i in doc.Descendants("channel").Elements("item")
                    select new RssItem
                    {
                        Title = i.Element("title").Value,
                        Link = i.Element("link").Value,
                        Description = i.Element("description").Value
                    }).ToList();
        }
    }

    [Serializable]
    public class RssItem
    {
        public string Title { get; set; }
        public string Link { get; set; }
        public string Description { get; set; }
    }
}
