using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Events
{
    public class RegisterUserEventArgs : MVCForumEventArgs
    {
        public MembershipUser User { get; set; }
        public MembershipCreateStatus CreateStatus { get; set; }
    }
}
