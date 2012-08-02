using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Data.Mapping
{
    public class LanguageMapping : EntityTypeConfiguration<Language>
    {
        public LanguageMapping()
        {
            HasKey(x => x.Id);

            HasMany(x => x.LocaleStringResources).WithRequired(x => x.Language)
                .WillCascadeOnDelete();
        }
    }
}
