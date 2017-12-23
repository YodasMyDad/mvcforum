using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{
    public class PollVoteMapping : EntityTypeConfiguration<PollVote>
    {
        public PollVoteMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
        }
    }
}
