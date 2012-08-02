using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Data.Mapping
{
    public class LocaleStringResourceMapping : EntityTypeConfiguration<LocaleStringResource>
    {
        public LocaleStringResourceMapping()
        {
            HasKey(x => x.Id);

            HasRequired(x => x.LocaleResourceKey)
                .WithMany()
                .Map(x => x.MapKey("LocaleResourceKey_Id"));

            HasRequired(x => x.Language)
                .WithMany()
                .Map(x => x.MapKey("Language_Id"));
        }
    }
}
