using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure.Annotations;
using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{
    public class CategoryMapping : EntityTypeConfiguration<Category>
    {
        public CategoryMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.Name).IsRequired().HasMaxLength(450);
            Property(x => x.Description).IsOptional();
            Property(x => x.DateCreated).IsRequired();
            Property(x => x.Slug).IsRequired().HasMaxLength(450)
                                .HasColumnAnnotation("Index",
                                new IndexAnnotation(new IndexAttribute("IX_Category_Slug", 1) { IsUnique = true }));
            Property(x => x.SortOrder).IsRequired();
            Property(x => x.IsLocked).IsRequired();
            Property(x => x.ModerateTopics).IsRequired();
            Property(x => x.ModeratePosts).IsRequired();
            Property(x => x.PageTitle).IsOptional().HasMaxLength(80);
            Property(x => x.MetaDescription).IsOptional().HasMaxLength(200);
            Property(x => x.Path).IsOptional().HasMaxLength(2500);
            Property(x => x.Colour).IsOptional().HasMaxLength(50);
            Property(x => x.Image).IsOptional().HasMaxLength(200);

            HasOptional(x => x.ParentCategory)
                .WithMany()
                .Map(x => x.MapKey("Category_Id"));


            HasMany(x => x.CategoryNotifications)
                .WithRequired(x => x.Category)
                .Map(x => x.MapKey("Category_Id"))
                .WillCascadeOnDelete(false);

            // Ignores
            Ignore(x => x.NiceUrl);
            Ignore(x => x.Level);
        }
    }
}
