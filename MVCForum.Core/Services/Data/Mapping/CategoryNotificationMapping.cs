using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{
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
