using System;
using System.Collections.Generic;
using System.Web.Mvc;
using System.Xml;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Website.Application
{
    public class GoogleSitemapResult : ActionResult
    {
        private readonly List<SitemapEntry> _items;

        public GoogleSitemapResult(IEnumerable<SitemapEntry> items)
        {
            _items = new List<SitemapEntry>(items);
        }

        public override void ExecuteResult(ControllerContext context)
        {
            var settings = new XmlWriterSettings { Indent = true, NewLineHandling = NewLineHandling.Entitize };

            context.HttpContext.Response.ContentType = "text/xml";
            using (var _writer = XmlWriter.Create(context.HttpContext.Response.OutputStream, settings))
            {
                var currentUrl = context.HttpContext.Request.Url.GetLeftPart(UriPartial.Authority);

                // Begin structure
                _writer.WriteStartElement("urlset", "http://www.sitemaps.org/schemas/sitemap/0.9");

                // Individual items
                _items.ForEach(x =>
                {
                    _writer.WriteStartElement("url");
                    _writer.WriteElementString("loc", string.Concat(currentUrl, x.Url));
                    _writer.WriteElementString("lastmod", string.Concat(x.LastUpdated.ToString("s"), "+00:00"));
                    _writer.WriteElementString("changefreq", x.ChangeFrequency.ToString());
                    _writer.WriteElementString("priority",  string.IsNullOrEmpty(x.Priority) ? "0.5" : x.Priority);
                    _writer.WriteEndElement();
                });

                // End structure
                _writer.WriteEndElement();
            }
        }
    }
}