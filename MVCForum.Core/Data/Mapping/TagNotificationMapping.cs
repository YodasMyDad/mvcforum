namespace MvcForum.Core.Data.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using Models.Entities;

    public class TagNotificationMapping : EntityTypeConfiguration<TagNotification>
    {
        public TagNotificationMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();

            HasRequired(x => x.Tag)
                .WithMany(x => x.Notifications)
                .Map(x => x.MapKey("TopicTag_Id"))
                .WillCascadeOnDelete(false);
        }
    }
}
