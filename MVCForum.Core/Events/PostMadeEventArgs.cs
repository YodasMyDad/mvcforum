namespace MvcForum.Core.Events
{
    using DomainModel;
    using DomainModel.Entities;

    public class PostMadeEventArgs : MvcForumEventArgs
    {
        public Post Post { get; set; }
    }
}