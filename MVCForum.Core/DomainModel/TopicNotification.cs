using System;
using System.Collections.Generic;

namespace MVCForum.Domain.DomainModel
{
    public class TopicNotification : Entity
    {
        public Guid Id { get; set; }
        public virtual Topic Topic { get; set; }
        public virtual MembershipUser User { get; set; }
    }
}
