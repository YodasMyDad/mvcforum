namespace MvcForum.Core.Events
{
    using DomainModel.Entities;
    using DomainModel.Enums;

    public class RegisterUserEventArgs : MvcForumEventArgs
    {
        public MembershipUser User { get; set; }
        public MembershipCreateStatus CreateStatus { get; set; }
    }
}