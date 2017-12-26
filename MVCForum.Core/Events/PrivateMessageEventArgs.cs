namespace MvcForum.Core.Events
{
    using DomainModel;
    using DomainModel.Entities;

    public class PrivateMessageEventArgs : MVCForumEventArgs
    {
        public PrivateMessage PrivateMessage { get; set; }
    }
}