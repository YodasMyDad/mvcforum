using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel.Entities;

namespace MVCForum.Services.Data.Mapping
{
    public class PostEditMapping : EntityTypeConfiguration<PostEdit>
    {
        public PostEditMapping()
        {
            HasKey(x => x.Id);

            Property(x => x.Id).IsRequired();
            Property(x => x.DateEdited).IsRequired();
            Property(x => x.OriginalPostContent).IsOptional().HasMaxLength(6100);
            Property(x => x.EditedPostContent).IsOptional().HasMaxLength(6100);
            Property(x => x.OriginalPostTitle).IsOptional().HasMaxLength(500);
            Property(x => x.EditedPostTitle).IsOptional().HasMaxLength(500);
        }
    }
}
