using System;
using System.Collections.Generic;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    public class TopicTag : Entity
    {
        public TopicTag()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public string Tag { get; set; }
        public string Slug { get; set; }

        public string NiceUrl
        {
            get { return UrlTypes.GenerateUrl(UrlType.Tag, Slug); }
        }

        public virtual IList<Topic> Topics { get; set; }
    }
}
