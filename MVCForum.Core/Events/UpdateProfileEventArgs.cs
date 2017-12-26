namespace MvcForum.Core.Events
{
    using DomainModel;
    using DomainModel.Entities;

    public class UpdateProfileEventArgs : MVCForumEventArgs
    {
        public MembershipUser User { get; set; }
    }
}