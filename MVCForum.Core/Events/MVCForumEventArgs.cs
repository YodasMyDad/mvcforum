using System;

namespace MVCForum.Domain.Events
{
    public abstract class MVCForumEventArgs : EventArgs
    {
        public bool Cancel { get; set; }
    }
}
