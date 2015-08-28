using System;
using System.Collections.Generic;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    public partial class Poll : Entity
    {
        public Poll()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public bool IsClosed { get; set; }
        public DateTime DateCreated { get; set; }
        public int? ClosePollAfterDays { get; set; }

        public virtual MembershipUser User { get; set; }
        public virtual IList<PollAnswer> PollAnswers { get; set; } 
    }
}
