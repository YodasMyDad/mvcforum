namespace MvcForum.Web.ViewModels.Admin
{
    using System.Collections.Generic;
    using Core.Models.General;

    public class ListLogViewModel
    {
        public IList<LogEntry> LogFiles { get; set; }
    }
}