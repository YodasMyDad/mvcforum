using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{
    public class TopicTagMapping : EntityTypeConfiguration<TopicTag>
    {
        public TopicTagMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.Tag).IsRequired().HasMaxLength(100);
            Property(x => x.Description).IsOptional();
            Property(x => x.Slug).IsRequired().HasMaxLength(100).HasColumnAnnotation("Index",
                                    new IndexAnnotation(new IndexAttribute("IX_Tag_Slug", 1) { IsUnique = true }));
        }
    }
}
