using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Events
{
    public class VoteEventArgs  : MVCForumEventArgs
    {
        public Vote Vote { get; set; }
    }
}
