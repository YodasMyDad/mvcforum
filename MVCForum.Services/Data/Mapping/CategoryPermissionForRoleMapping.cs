using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{
    public class CategoryPermissionForRoleMapping : EntityTypeConfiguration<CategoryPermissionForRole>
    {
        public CategoryPermissionForRoleMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.IsTicked).IsRequired();
            HasRequired(x => x.Category).WithMany(x => x.CategoryPermissionForRoles).Map(x => x.MapKey("Category_Id")).WillCascadeOnDelete(false);
            HasRequired(x => x.Permission).WithMany(x => x.CategoryPermissionForRoles).Map(x => x.MapKey("Permission_Id")).WillCascadeOnDelete(false);
            HasRequired(x => x.MembershipRole).WithMany(x => x.CategoryPermissionForRoles).Map(x => x.MapKey("MembershipRole_Id")).WillCascadeOnDelete(false);
        }
    }
}
