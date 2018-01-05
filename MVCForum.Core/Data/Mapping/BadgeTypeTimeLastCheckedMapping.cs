namespace MvcForum.Core.Data.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using Models.Entities;

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