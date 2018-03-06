namespace MvcForum.Core.Events
{
    using Models.Entities;
    using Models.Enums;

    public class BadgeEventArgs : MvcForumEventArgs
    {
        public MembershipUser User { get; set; }
        public BadgeType BadgeType { get; set; }
    }
}