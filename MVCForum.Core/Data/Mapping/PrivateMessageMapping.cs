namespace MvcForum.Core.Data.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using Models.Entities;

    public class PrivateMessageMapping : EntityTypeConfiguration<PrivateMessage>
    {
        public PrivateMessageMapping()
        {
            HasKey(x => x.Id);                  
            Property(x => x.Id).IsRequired();
            Property(x => x.DateSent).IsRequired();
            Property(x => x.Message).IsRequired();
            Property(x => x.IsRead).IsRequired();
            Property(x => x.IsSentMessage).IsRequired();
        }
    }
}
