using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Data.Mapping
{
    public class PrivateMessageMapping : EntityTypeConfiguration<PrivateMessage>
    {
        public PrivateMessageMapping()
        {
            HasKey(x => x.Id);
            
                HasRequired(x => x.UserFrom)
                .WithMany()
                .Map(x => x.MapKey("UserFrom_Id"));

                HasRequired(x => x.UserTo).WithMany()
                   .Map(x => x.MapKey("UserTo_Id"));           
        }
    }
}
