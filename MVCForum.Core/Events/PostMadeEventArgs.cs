namespace MvcForum.Core.Events
{
    using DomainModel;
    using Models.Entities;

    public class PostMadeEventArgs : MvcForumEventArgs
    {
        public Post Post { get; set; }
    }
}