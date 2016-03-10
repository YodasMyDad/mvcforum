using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Website.ViewModels
{
    public class ModerateViewModel
    {
        public IList<Topic> Topics { get; set; }
        public IList<Post> Posts { get; set; }
    }

    public class ModerateActionViewModel
    {
        public bool IsApproved { get; set; }
        public Guid TopicId { get; set; }
        public Guid PostId { get; set; }
    }
}