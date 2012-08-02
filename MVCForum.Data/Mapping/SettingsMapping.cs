using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Data.Mapping
{
    public class SettingsMapping : EntityTypeConfiguration<Settings>
    {
        public SettingsMapping()
        {
            HasKey(x => x.Id);

            HasRequired(t => t.NewMemberStartingRole)
                .WithOptional(x => x.Settings).Map(m => m.MapKey("NewMemberStartingRole"));

            HasRequired(x => x.DefaultLanguage)
                .WithOptional().Map(m => m.MapKey("DefaultLanguage_Id"));
        }
    }
}
