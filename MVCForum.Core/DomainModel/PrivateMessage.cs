using System;
using MVCForum.Utilities;

namespace MVCForum.Domain.DomainModel
{
    public class PrivateMessage : Entity
    {
        public PrivateMessage()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public DateTime DateSent { get; set; }
        public string Subject { get; set; }
        public string Message { get; set; }
        public bool IsRead { get; set; }
        public bool? IsSentMessage { get; set; }
        public virtual MembershipUser UserTo { get; set; }
        public virtual MembershipUser UserFrom { get; set; }
    }
}
