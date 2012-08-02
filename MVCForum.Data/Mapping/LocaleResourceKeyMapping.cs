using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Data.Mapping
{
    public class LocaleResourceKeyMapping : EntityTypeConfiguration<LocaleResourceKey>
    {
        public LocaleResourceKeyMapping()
        {
            HasKey(x => x.Id);

            HasMany(x => x.LocaleStringResources).WithRequired(x => x.LocaleResourceKey)                
                .WillCascadeOnDelete();
        }
    }
}
