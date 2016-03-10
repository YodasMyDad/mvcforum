using System.Data.Entity.ModelConfiguration;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Services.Data.Mapping
{
    public class UploadedFileMapping : EntityTypeConfiguration<UploadedFile>
    {
        public UploadedFileMapping()
        {
            HasKey(x => x.Id);
            Property(x => x.Id).IsRequired();
            Property(x => x.Filename).IsRequired().HasMaxLength(200);
            Property(x => x.DateCreated).IsRequired();

            HasOptional(x => x.Post).WithMany(x => x.Files).Map(x => x.MapKey("Post_Id")).WillCascadeOnDelete(false);
        }
    }
}
