namespace MvcForum.Core.Models.Entities
{
    using System;
    using Interfaces;
    using Utilities;

    public partial class Vote : IBaseEntity
    {
        public Vote()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public int Amount { get; set; }

        public virtual MembershipUser User { get; set; }
        public virtual Post Post { get; set; }
        public virtual MembershipUser VotedByMembershipUser { get; set; }
        public virtual DateTime? DateVoted { get; set; }
    }
}
