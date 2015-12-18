using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{
    public class BannedEmailMapping : EntityTypeConfiguration<BannedEmail>
    {
        public BannedEmailMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.Email).IsRequired().HasMaxLength(200);
            Property(x => x.DateAdded).IsRequired();
        }
    }
}
