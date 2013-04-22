using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Data.Mapping
{
    public class BannedEmailMapping : EntityTypeConfiguration<BannedEmail>
    {
        public BannedEmailMapping()
        {
            HasKey(x => x.Id);
        }
    }
}
