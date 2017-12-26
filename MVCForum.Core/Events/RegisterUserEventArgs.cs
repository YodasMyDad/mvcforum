namespace MvcForum.Core.Events
{
    using DomainModel.Entities;
    using DomainModel.Enums;

    public class RegisterUserEventArgs : MVCForumEventArgs
    {
        public MembershipUser User { get; set; }
        public MembershipCreateStatus CreateStatus { get; set; }
    }
}