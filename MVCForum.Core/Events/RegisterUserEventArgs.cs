namespace MvcForum.Core.Events
{
    using Models.Entities;
    using Models.Enums;

    public class RegisterUserEventArgs : MvcForumEventArgs
    {
        public MembershipUser User { get; set; }
        public MembershipCreateStatus CreateStatus { get; set; }
    }
}