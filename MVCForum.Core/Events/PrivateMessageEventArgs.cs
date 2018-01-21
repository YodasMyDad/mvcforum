namespace MvcForum.Core.Events
{
    using Models.Entities;

    public class PrivateMessageEventArgs : MvcForumEventArgs
    {
        public PrivateMessage PrivateMessage { get; set; }
    }
}