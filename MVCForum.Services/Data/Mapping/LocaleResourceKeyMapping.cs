using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{
    public class LocaleResourceKeyMapping : EntityTypeConfiguration<LocaleResourceKey>
    {
        public LocaleResourceKeyMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.Name).IsRequired().HasMaxLength(200);
            Property(x => x.Notes).IsOptional();
            Property(x => x.DateAdded).IsRequired();

            HasMany(x => x.LocaleStringResources).WithRequired(x => x.LocaleResourceKey)
                .Map(x => x.MapKey("LocaleResourceKey_Id"))
                .WillCascadeOnDelete(false);
        }
    }
}
