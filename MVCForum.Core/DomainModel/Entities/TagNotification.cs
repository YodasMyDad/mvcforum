using System;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    public partial class TagNotification : Entity
    {
        public TagNotification()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public virtual TopicTag Tag { get; set; }
        public virtual MembershipUser User { get; set; }
    }
}
