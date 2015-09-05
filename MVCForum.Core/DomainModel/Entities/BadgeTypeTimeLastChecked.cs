using System;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{

    public partial class BadgeTypeTimeLastChecked : Entity
    {
        public BadgeTypeTimeLastChecked()
        {
            Id = GuidComb.GenerateComb();
        }

        public Guid Id { get; set; }
        public string BadgeType { get; set; }
        public DateTime TimeLastChecked { get; set; }

        public virtual MembershipUser User { get; set; }
    }
}
