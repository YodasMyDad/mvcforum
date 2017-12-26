using System;

namespace MvcForum.Core.DomainModel.Activity
{
    using Entities;
    using Utilities;

    public enum ActivityType
    {
        BadgeAwarded,
        MemberJoined,
        ProfileUpdated,
    }

    public class Activity : Entity
    {
        public Activity()
        {
            Id = GuidComb.GenerateComb();
        }
        public Guid Id { get; set; }
        public string Type { get; set; }
        public string Data { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
