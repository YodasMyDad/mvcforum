using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Data.Mapping
{
    public class BannedWordMapping : EntityTypeConfiguration<BannedEmail>
    {
        public BannedWordMapping()
        {
            HasKey(x => x.Id);
        }
    }
}
