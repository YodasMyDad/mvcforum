using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Data.Mapping
{
    public class PostMapping : EntityTypeConfiguration<Post>
    {
        public PostMapping()
        {
            HasKey(x => x.Id);

            HasMany(x => x.Votes).WithRequired(x => x.Post)
                .Map(x => x.MapKey("Post_Id"))
                .WillCascadeOnDelete();
        }
    }
}
