namespace MvcForum.Core.Models.Entities
{
    using System;
    using System.Collections.Generic;
    using Utilities;

    public partial class PollAnswer : ExtendedDataEntity
    {
        public PollAnswer()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public string Answer { get; set; }
        public virtual Poll Poll { get; set; }
        public virtual IList<PollVote> PollVotes { get; set; }
    }
}
