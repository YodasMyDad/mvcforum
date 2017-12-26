namespace MvcForum.Core.Events
{
    using DomainModel;
    using DomainModel.Entities;

    public class VoteEventArgs : MvcForumEventArgs
    {
        public Vote Vote { get; set; }
    }
}