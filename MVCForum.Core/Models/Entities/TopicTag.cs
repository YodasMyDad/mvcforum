﻿namespace MvcForum.Core.Models.Entities
{
    using System;
    using System.Collections.Generic;
    using Enums;
    using Interfaces;
    using Utilities;

    public partial class TopicTag : ExtendedDataEntity, IBaseEntity
    {
        public TopicTag()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public string Tag { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }

        public string NiceUrl
        {
            get
            {
                var url = UrlTypes.GenerateUrl(UrlType.Tag, StringUtils.RemoveAccents(Slug));
                return url;
            }
        }

        public virtual IList<Topic> Topics { get; set; }
        public virtual IList<TagNotification> Notifications { get; set; }
    }
}
