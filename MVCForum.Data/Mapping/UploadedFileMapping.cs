using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Data.Mapping
{
    public class UploadedFileMapping : EntityTypeConfiguration<UploadedFile>
    {
        public UploadedFileMapping()
        {
            HasKey(x => x.Id);
            HasRequired(x => x.Post).WithMany(x => x.Files).Map(x => x.MapKey("Post_Id"));
            HasRequired(x => x.MembershipUser);
        }
    }
}
