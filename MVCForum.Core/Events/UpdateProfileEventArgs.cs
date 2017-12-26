namespace MvcForum.Core.Events
{
    using DomainModel;
    using DomainModel.Entities;

    public class UpdateProfileEventArgs : MvcForumEventArgs
    {
        public MembershipUser User { get; set; }
    }
}