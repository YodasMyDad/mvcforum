using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel.Entities;

namespace MVCForum.Services.Data.Mapping
{
    public class BlockMapping : EntityTypeConfiguration<Block>
    {
        public BlockMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.Date).IsRequired();
            HasRequired(x => x.Blocker).WithMany(x => x.BlockedUsers).Map(x => x.MapKey("Blocker_Id")).WillCascadeOnDelete(false);
            HasRequired(x => x.Blocked).WithMany(x => x.BlockedByOtherUsers).Map(x => x.MapKey("Blocked_Id")).WillCascadeOnDelete(false);
        }
    }
}
