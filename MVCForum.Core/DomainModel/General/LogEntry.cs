using System;

namespace MVCForum.Domain.DomainModel
{
    public partial class LogEntry
    {
        public DateTime Date { get; set; }
        public string Module { get; set;}
        public string Method { get; set; }
        public string ErrorMessage { get; set; }
    }
}
