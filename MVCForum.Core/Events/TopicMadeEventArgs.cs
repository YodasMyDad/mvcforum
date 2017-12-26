namespace MvcForum.Core.Events
{
    using DomainModel;
    using DomainModel.Entities;

    public class TopicMadeEventArgs : MvcForumEventArgs
    {
        public Topic Topic { get; set; }
    }
}