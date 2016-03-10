using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Events
{
    public class TopicMadeEventArgs : MVCForumEventArgs
    {
        public Topic Topic { get; set; }
    }
}
