namespace MvcForum.Core.Events
{
    using DomainModel;
    using Models.Entities;

    public class TopicMadeEventArgs : MvcForumEventArgs
    {
        public Topic Topic { get; set; }
    }
}