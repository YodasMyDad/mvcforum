using System;

namespace MvcForum.Core.Events
{
    public abstract class MVCForumEventArgs : EventArgs
    {
        public bool Cancel { get; set; }
    }
}
