namespace MvcForum.Core.Events
{
    using DomainModel;
    using DomainModel.Entities;

    public class PostMadeEventArgs : MVCForumEventArgs
    {
        public Post Post { get; set; }
    }
}