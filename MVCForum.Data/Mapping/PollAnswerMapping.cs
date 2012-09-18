using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Data.Mapping
{
    public class PollAnswerMapping : EntityTypeConfiguration<PollAnswer>
    {
        public PollAnswerMapping()
        {
            HasKey(x => x.Id);
            HasRequired(x => x.Poll).WithMany(t => t.PollAnswers).Map(m => m.MapKey("Poll_Id"));
            HasMany(x => x.PollVotes)
                .WithRequired(t => t.PollAnswer)
                .Map(x => x.MapKey("PollAnswer_Id"))
                .WillCascadeOnDelete();
        }
    }
}
