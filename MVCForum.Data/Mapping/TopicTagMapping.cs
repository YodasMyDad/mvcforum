using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Data.Mapping
{
    public class TopicTagMapping : EntityTypeConfiguration<TopicTag>
    {
        public TopicTagMapping()
        {
            HasKey(x => x.Id);

            HasMany(t => t.Topics)
                .WithMany(t => t.Tags)
                .Map(m =>
                {
                    m.ToTable("Topic_Tag");
                    m.MapLeftKey("Topic_Id");
                    m.MapRightKey("TopicTag_Id");
                });
        }
    }
}
