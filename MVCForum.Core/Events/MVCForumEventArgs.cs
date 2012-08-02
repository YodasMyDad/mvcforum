using System;
using MVCForum.Domain.Interfaces.API;

namespace MVCForum.Domain.Events
{
    public abstract class MVCForumEventArgs : EventArgs
    {
        public IMVCForumAPI Api { get; set; }
        public bool Cancel { get; set; }
    }
}
