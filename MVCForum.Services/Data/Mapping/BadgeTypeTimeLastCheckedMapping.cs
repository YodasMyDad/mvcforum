using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{
    public class BadgeTypeTimeLastCheckedMapping : EntityTypeConfiguration<BadgeTypeTimeLastChecked>
    {
        public BadgeTypeTimeLastCheckedMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.BadgeType).IsRequired().HasMaxLength(50);
            Property(x => x.TimeLastChecked).IsRequired();
            HasRequired(t => t.User)
                .WithMany(t => t.BadgeTypesTimeLastChecked)
                .Map(m => m.MapKey("MembershipUser_Id"))
                .WillCascadeOnDelete(false);
        }
    }
}
