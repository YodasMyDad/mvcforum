namespace MvcForum.Core.Models.General
{
    using System;
    using Entities;
    using Utilities;

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
        public string FilePath => $"{ForumConfiguration.Instance.UploadFolderPath}{MembershipUser.Id}/{Filename}";
    }
}
