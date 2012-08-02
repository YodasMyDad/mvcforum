using System;

namespace MVCForum.Domain.DomainModel
{

    public class BadgeTypeTimeLastChecked : Entity
    {    
        public Guid Id { get; set; }
        public string BadgeType { get; set; }
        public DateTime TimeLastChecked { get; set; }

        public virtual MembershipUser User { get; set; }
    }
}
