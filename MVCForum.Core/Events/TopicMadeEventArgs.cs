namespace MvcForum.Core.Events
{
    using Models.Entities;

    public class TopicMadeEventArgs : MvcForumEventArgs
    {
        public Topic Topic { get; set; }
    }
}