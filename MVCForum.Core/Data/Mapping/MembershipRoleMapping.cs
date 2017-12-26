namespace MvcForum.Core.Data.Mapping
{
    using System.Data.Entity.ModelConfiguration;
    using DomainModel;
    using DomainModel.Entities;

    public class MembershipRoleMapping : EntityTypeConfiguration<MembershipRole>
    {
        public MembershipRoleMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.RoleName).IsRequired().HasMaxLength(256);
        }
    }
}
