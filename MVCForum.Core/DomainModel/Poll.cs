using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    public class Poll : Entity
    {
        public Poll()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public string Question { get; set; }
        public bool IsClosed { get; set; }
        public DateTime DateCreated { get; set; }

        public virtual MembershipUser User { get; set; }
        public virtual IList<PollAnswer> PollAnswers { get; set; } 
    }
}
