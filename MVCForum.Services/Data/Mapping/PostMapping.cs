using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{
    public class PostMapping : EntityTypeConfiguration<Post>
    {
        public PostMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.PostContent).IsRequired();
            Property(x => x.DateCreated).IsRequired();
            Property(x => x.VoteCount).IsRequired();
            Property(x => x.DateEdited).IsRequired();
            Property(x => x.IsSolution).IsRequired();
            Property(x => x.IsTopicStarter).IsOptional();
            Property(x => x.FlaggedAsSpam).IsOptional();
            Property(x => x.IpAddress).IsOptional().HasMaxLength(50);
            Property(x => x.Pending).IsOptional();
            Property(x => x.SearchField).IsOptional();
            Property(x => x.InReplyTo).IsOptional();

            HasMany(x => x.Votes).WithRequired(x => x.Post)
                .Map(x => x.MapKey("Post_Id"))
                .WillCascadeOnDelete(false);
            HasMany(x => x.PostEdits)
                .WithRequired(x => x.Post)
                .Map(x => x.MapKey("Post_Id"))
                .WillCascadeOnDelete(false);
            //ToTable("CustomTableName");
            //Property(t => t.TopicId).HasColumnName("Topic_Id");
        }
    }
}
