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

            HasMany(t => t.Users)
            .WithMany(t => t.Badges)
            .Map(m =>
                {
                    m.ToTable("MembershipUser_Badge");
                    m.MapLeftKey("Badge_Id");
                    m.MapRightKey("MembershipUser_Id");
                });
        }
    }
}
