namespace MvcForum.Core.Events
{
    using Models.Entities;

    public class UpdateProfileEventArgs : MvcForumEventArgs
    {
        public MembershipUser User { get; set; }
    }
}