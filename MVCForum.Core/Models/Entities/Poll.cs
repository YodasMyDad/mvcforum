namespace MvcForum.Core.Models.Entities
{
    using System;
    using System.Collections.Generic;
    using Utilities;

    public partial class Poll : ExtendedDataEntity
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
        public virtual List<PollAnswer> PollAnswers { get; set; } 
    }
}
