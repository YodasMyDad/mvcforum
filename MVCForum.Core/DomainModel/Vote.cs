using System;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    public class Vote : Entity
    {
        public Vote()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public int Amount { get; set; }

        public virtual MembershipUser User { get; set; }
        public virtual Post Post { get; set; }
    }
}
