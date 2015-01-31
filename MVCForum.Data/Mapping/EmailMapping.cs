using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Data.Mapping
{
    public class EmailMapping : EntityTypeConfiguration<Email>
    {
        public EmailMapping()
        {
            HasKey(x => x.Id);
        }
    }
}
