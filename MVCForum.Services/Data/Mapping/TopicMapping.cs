using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{
    public class TopicMapping : EntityTypeConfiguration<Topic>
    {
        public TopicMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.Name).IsRequired().HasMaxLength(450);
            Property(x => x.CreateDate).IsRequired();
            Property(x => x.Solved).IsRequired();
            Property(x => x.SolvedReminderSent).IsOptional();
            Property(x => x.Slug).IsRequired().HasMaxLength(450).HasColumnAnnotation("Index",
                                    new IndexAnnotation(new IndexAttribute("IX_Topic_Slug", 1) { IsUnique = true }));
            Property(x => x.Views).IsOptional();
            Property(x => x.IsSticky).IsRequired();
            Property(x => x.IsLocked).IsRequired();
            Property(x => x.Pending).IsOptional();

            // LastPost is not really optional but causes a circular dependency so needs to be added in after the main post is saved
            HasOptional(t => t.LastPost).WithOptionalDependent().Map(m => m.MapKey("Post_Id")).WillCascadeOnDelete(false);
            HasOptional(t => t.Poll).WithOptionalDependent().Map(m => m.MapKey("Poll_Id"));            
            HasRequired(t => t.Category).WithMany(t => t.Topics).Map(m => m.MapKey("Category_Id")).WillCascadeOnDelete(false);
            HasRequired(t => t.User).WithMany(t => t.Topics).Map(m => m.MapKey("MembershipUser_Id")).WillCascadeOnDelete(false);
            HasMany(x => x.Posts).WithRequired(x => x.Topic).Map(x => x.MapKey("Topic_Id")).WillCascadeOnDelete(false);
            HasMany(x => x.TopicNotifications).WithRequired(x => x.Topic).Map(x => x.MapKey("Topic_Id")).WillCascadeOnDelete(false);
            HasMany(t => t.Tags)
            .WithMany(t => t.Topics)
            .Map(m =>
                    {
                        m.ToTable("Topic_Tag");
                        m.MapLeftKey("Topic_Id");
                        m.MapRightKey("TopicTag_Id");
                    });
        }
    }
}
