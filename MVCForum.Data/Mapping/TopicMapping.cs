using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Data.Mapping
{
    public class TopicMapping : EntityTypeConfiguration<Topic>
    {
        public TopicMapping()
        {
            HasKey(x => x.Id);

            // LastPost is not really optional but causes a circular dependency so needs to be added in after the main post is saved
            HasOptional(t => t.LastPost).WithOptionalDependent().Map(m => m.MapKey("Post_Id"));
            HasOptional(t => t.Poll).WithOptionalDependent().Map(m => m.MapKey("Poll_Id"));            
            HasRequired(t => t.Category).WithMany(t => t.Topics).Map(m => m.MapKey("Category_Id"));
            HasRequired(t => t.User).WithMany(t => t.Topics).Map(m => m.MapKey("MembershipUser_Id"));
            HasMany(x => x.Posts).WithRequired(x => x.Topic).Map(x => x.MapKey("Topic_Id")).WillCascadeOnDelete();
            HasMany(x => x.TopicNotifications).WithRequired(x => x.Topic).Map(x => x.MapKey("Topic_Id")).WillCascadeOnDelete();
            HasMany(t => t.Tags)
            .WithMany(t => t.Topics)
            .Map(m =>
                    {
                        m.ToTable("Topic_Tag");
                        m.MapLeftKey("TopicTag_Id");
                        m.MapRightKey("Topic_Id");
                    });
        }
    }
}
