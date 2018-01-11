namespace MvcForum.Core.Models.Entities
{
    using System;
    using Utilities;

    public partial class TagNotification : IBaseEntity
    {
        public TagNotification()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public virtual TopicTag Tag { get; set; }
        public virtual MembershipUser User { get; set; }
    }
}
