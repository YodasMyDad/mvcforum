using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Events
{
    public class PostMadeEventArgs : MVCForumEventArgs
    {
        public Post Post { get; set; }
    }
}
