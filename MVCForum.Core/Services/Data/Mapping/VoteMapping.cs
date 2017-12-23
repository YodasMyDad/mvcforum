using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{
    public class VoteMapping : EntityTypeConfiguration<Vote>
    {
        public VoteMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.Amount).IsRequired();
            Property(x => x.DateVoted).IsOptional();
        }
    }
}
