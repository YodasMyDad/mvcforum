using System;
using System.Collections.Generic;

namespace MVCForum.Domain.DomainModel
{
    public class CategoryNotification : Entity
    {
        public Guid Id { get; set; }
        public virtual Category Category { get; set; }
        public virtual MembershipUser User { get; set; }
    }
}
