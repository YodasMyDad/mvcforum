namespace MvcForum.Core.Data.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using Models.Entities;

    public class BannedEmailMapping : EntityTypeConfiguration<BannedEmail>
    {
        public BannedEmailMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.Email).IsRequired().HasMaxLength(200);
            Property(x => x.DateAdded).IsRequired();
        }
    }
}