using System;

namespace MVCForum.Domain.DomainModel.Activity
{
    public enum ActivityType
    {
        BadgeAwarded,
        MemberJoined,
        ProfileUpdated,
    }

    public class Activity : Entity
    {
        public virtual Guid Id { get; set; }
        public virtual string Type { get; set; }
        public virtual string Data { get; set; }
        public virtual DateTime Timestamp { get; set; }
    }
}
