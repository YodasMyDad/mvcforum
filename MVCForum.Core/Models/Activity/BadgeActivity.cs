namespace MvcForum.Core.Models.Activity
{
    using System;
    using Entities;
    using Enums;

    public class BadgeActivity : ActivityBase
    {
        public const string KeyBadgeId = @"BadgeID";
        public const string KeyUserId = @"UserID";

        public Badge Badge { get; set; }
        public MembershipUser User { get; set; }

        /// <summary>
        /// Constructor - useful when constructing a badge activity after reading database
        /// </summary>
        public BadgeActivity(Activity activity, Badge badge, MembershipUser user)
        {
            ActivityMapped = activity;
            Badge = badge;
            User = user;
        }

        public static Activity GenerateMappedRecord(Badge badge, MembershipUser user, DateTime timestamp)
        {
            return new Activity
                       {
                           // badge=badgeId,user=userId
                           Data = KeyBadgeId + Equality + badge.Id + Separator + KeyUserId + Equality + user.Id,
                           Timestamp = timestamp,
                           Type = ActivityType.BadgeAwarded.ToString()
                       };

        }
    }
}
