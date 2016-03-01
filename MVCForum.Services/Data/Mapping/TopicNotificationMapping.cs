using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{
    public class TopicNotificationMapping : EntityTypeConfiguration<TopicNotification>
    {
        public TopicNotificationMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
        }
    }
}
