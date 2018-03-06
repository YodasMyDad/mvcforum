namespace MvcForum.Core.Events
{
    using Models.Entities;

    public class VoteEventArgs : MvcForumEventArgs
    {
        public Vote Vote { get; set; }
    }
}