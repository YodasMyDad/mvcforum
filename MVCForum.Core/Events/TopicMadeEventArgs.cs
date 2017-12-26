namespace MvcForum.Core.Events
{
    using DomainModel;
    using DomainModel.Entities;

    public class TopicMadeEventArgs : MVCForumEventArgs
    {
        public Topic Topic { get; set; }
    }
}