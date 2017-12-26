namespace MvcForum.Core.Events
{
    using DomainModel;
    using DomainModel.Entities;

    public class VoteEventArgs : MVCForumEventArgs
    {
        public Vote Vote { get; set; }
    }
}