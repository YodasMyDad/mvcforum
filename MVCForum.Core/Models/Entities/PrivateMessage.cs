namespace MvcForum.Core.Models.Entities
{
    using System;
    using Interfaces;
    using Utilities;

    public partial class PrivateMessage : IBaseEntity
    {
        public PrivateMessage()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public DateTime DateSent { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public bool? IsSentMessage { get; set; }
        public virtual MembershipUser UserTo { get; set; }
        public virtual MembershipUser UserFrom { get; set; }
    }

    public partial class PrivateMessageListItem
    {
        public MembershipUser User { get; set; }
        public DateTime Date { get; set; }
        public bool HasUnreadMessages { get; set; }
    }
}
