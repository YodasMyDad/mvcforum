using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Events
{
    public class UpdateProfileEventArgs : MVCForumEventArgs
    {
        public MembershipUser User { get; set; }
    }
}
