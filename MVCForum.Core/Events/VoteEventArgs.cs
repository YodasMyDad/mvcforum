namespace MvcForum.Core.Events
{
    using DomainModel;
    using Models.Entities;

    public class VoteEventArgs : MvcForumEventArgs
    {
        public Vote Vote { get; set; }
    }
}