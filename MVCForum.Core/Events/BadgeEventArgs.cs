namespace MvcForum.Core.Events
{
    using DomainModel;
    using DomainModel.Entities;

    public class BadgeEventArgs : MvcForumEventArgs
    {
        public MembershipUser User { get; set; }
        public BadgeType BadgeType { get; set; }
    }
}