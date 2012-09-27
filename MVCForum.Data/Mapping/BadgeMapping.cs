using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Data.Mapping
{
    public class BadgeMapping : EntityTypeConfiguration<Badge>
    {
        public BadgeMapping()
        {
            HasKey(x => x.Id);
            Ignore(x => x.Milestone);
        }
    }
}
