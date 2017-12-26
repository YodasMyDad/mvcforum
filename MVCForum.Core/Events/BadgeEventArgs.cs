namespace MvcForum.Core.Events
{
    using DomainModel;
    using DomainModel.Entities;

    public class BadgeEventArgs : MVCForumEventArgs
    {
        public MembershipUser User { get; set; }
        public BadgeType BadgeType { get; set; }
    }
}