using System;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    public partial class UploadedFile
    {
        public UploadedFile()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public string Filename { get; set; }
        public virtual MembershipUser MembershipUser { get; set; }
        public virtual Post Post { get; set; }
        public DateTime DateCreated { get; set; }

        public string FriendlyFilename
        {
            get { return Filename.Split('_')[1]; }
        }
        public string FilePath
        {
            get { return string.Format("~/content/uploads/{0}/{1}", MembershipUser.Id, Filename); }
        }
    }
}
