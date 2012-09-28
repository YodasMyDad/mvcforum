using System;
using System.Collections.Generic;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Data.Mapping
{
    public class PollMapping : EntityTypeConfiguration<Poll>
    {
        public PollMapping()
        {
            HasKey(x => x.Id);

            HasRequired(x => x.User).WithMany(t => t.Polls).Map(m => m.MapKey("MembershipUser_Id"));

            HasMany(x => x.PollAnswers)
                .WithRequired(t => t.Poll)
                .Map(x => x.MapKey("Poll_Id"))
                .WillCascadeOnDelete();
        }
    }
}
