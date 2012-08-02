using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Events
{
    public class BadgeEventArgs : MVCForumEventArgs
    {
        public MembershipUser User { get; set; }
        public BadgeType BadgeType { get; set; }

    }
}
