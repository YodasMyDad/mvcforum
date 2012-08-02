using System;

namespace MVCForum.Domain.DomainModel
{
    public class MembershipUserPoints : Entity
    {
        public Guid Id { get; set; }
        public int Points { get; set; }
        public DateTime DateAdded { get; set; }

        public virtual MembershipUser User { get; set; }
    }
}
