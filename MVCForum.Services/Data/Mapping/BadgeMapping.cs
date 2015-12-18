using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{
    public class BadgeMapping : EntityTypeConfiguration<Badge>
    {
        public BadgeMapping()
        {
            HasKey(x => x.Id);

            Property(x => x.Id).IsRequired();
            Property(x => x.Name).IsRequired().HasMaxLength(50);
            Property(x => x.Description).IsOptional();
            Property(x => x.Type).IsRequired().HasMaxLength(50);
            Property(x => x.Image).IsOptional().HasMaxLength(50);
            Property(x => x.DisplayName).IsRequired().HasMaxLength(50);
            Property(x => x.AwardsPoints).IsOptional();

            // Ignores
            Ignore(x => x.Milestone);

            // TODO - Change Table Names
            //ToTable("ForumBadge"); 
        }
    }
}
