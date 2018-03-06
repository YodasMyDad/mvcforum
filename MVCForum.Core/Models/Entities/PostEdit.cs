namespace MvcForum.Core.Models.Entities
{
    using System;
    using Interfaces;
    using Utilities;

    public partial class PostEdit : IBaseEntity
    {
        public PostEdit()
        {
            Id = GuidComb.GenerateComb();
        }

        public Guid Id { get; set; }
        public DateTime DateEdited { get; set; }
        public string OriginalPostContent { get; set; }
        public string EditedPostContent { get; set; }
        public string OriginalPostTitle { get; set; }
        public string EditedPostTitle { get; set; }
        public virtual MembershipUser EditedBy { get; set; }
        public virtual Post Post { get; set; }
    }
}