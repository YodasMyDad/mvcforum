using System;
using System.Collections.Generic;

namespace MVCForum.Domain.DomainModel
{
    public class TopicTag : Entity
    {
        public Guid Id { get; set; }  
        public string Tag { get; set; }

        public virtual IList<Topic> Topics { get; set; }
    }
}
