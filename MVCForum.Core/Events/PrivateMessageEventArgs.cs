namespace MvcForum.Core.Events
{
    using DomainModel;
    using DomainModel.Entities;

    public class PrivateMessageEventArgs : MvcForumEventArgs
    {
        public PrivateMessage PrivateMessage { get; set; }
    }
}