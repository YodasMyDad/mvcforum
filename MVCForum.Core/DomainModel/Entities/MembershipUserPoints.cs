using System;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    public partial class MembershipUserPoints : Entity
    {
        public MembershipUserPoints()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public int Points { get; set; }
        public DateTime DateAdded { get; set; }

        public virtual MembershipUser User { get; set; }
    }
}
