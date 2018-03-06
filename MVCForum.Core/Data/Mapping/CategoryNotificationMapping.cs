namespace MvcForum.Core.Data.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using Models.Entities;

    public class CategoryNotificationMapping : EntityTypeConfiguration<CategoryNotification>
    {
        public CategoryNotificationMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();

            HasRequired(x => x.Category)
                .WithMany(x => x.CategoryNotifications)
                .Map(x => x.MapKey("Category_Id"))
                .WillCascadeOnDelete(false);

        }
    }
}
