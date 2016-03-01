using System;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel.Entities
{
    public partial class PostEdit : Entity
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
