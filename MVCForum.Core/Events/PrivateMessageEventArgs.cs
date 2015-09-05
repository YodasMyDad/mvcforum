using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Events
{
    public class PrivateMessageEventArgs : MVCForumEventArgs
    {
        public PrivateMessage PrivateMessage { get; set; }
    }
}
