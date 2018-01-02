namespace MvcForum.Core.Events
{
    using System;

    public abstract class MvcForumEventArgs : EventArgs
    {
        public bool Cancel { get; set; }
    }
}