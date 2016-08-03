using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{
    public class MembershipFirmMapping : EntityTypeConfiguration<MembershipFirm>
    {
        public MembershipFirmMapping()
        {
            HasKey(x => x.Id);
        }
    }
}
