using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Data.Mapping
{
    public class GlobalPermissionForRoleMapping : EntityTypeConfiguration<GlobalPermissionForRole>
    {
        public GlobalPermissionForRoleMapping()
        {
            HasKey(x => x.Id);
            HasRequired(x => x.Permission);
            HasRequired(x => x.MembershipRole);
        }
    }
}
