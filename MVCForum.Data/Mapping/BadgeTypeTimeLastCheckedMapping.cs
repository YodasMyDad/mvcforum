using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Data.Mapping
{
    public class BadgeTypeTimeLastCheckedMapping : EntityTypeConfiguration<BadgeTypeTimeLastChecked>
    {
        public BadgeTypeTimeLastCheckedMapping()
        {
            HasKey(x => x.Id);
            HasRequired(t => t.User)
                .WithMany(t => t.BadgeTypesTimeLastChecked)
                .Map(m => m.MapKey("MembershipUser_Id"));
        }
    }
}
