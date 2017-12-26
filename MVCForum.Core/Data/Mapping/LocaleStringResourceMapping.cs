namespace MvcForum.Core.Data.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using DomainModel;
    using DomainModel.Entities;

    public class LocaleStringResourceMapping : EntityTypeConfiguration<LocaleStringResource>
    {
        public LocaleStringResourceMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.ResourceValue).IsRequired().HasMaxLength(1000);
        }
    }
}
