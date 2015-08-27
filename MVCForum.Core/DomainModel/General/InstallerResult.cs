using System;

namespace MVCForum.Domain.DomainModel
{
    public partial class InstallerResult
    {
        public string Message { get; set; }
        public bool Successful { get; set; }
        public Exception Exception { get; set; }
        public string OnScreenMessage 
        {
            get
            {
                var exMessage = Exception != null ? string.Format("<br /><strong>Error Message:<strong> {0}<br />{1}", Exception.Message, Exception.InnerException) : string.Empty;
                return string.Format("<p><strong>{0}<strong>{1}</p>", Message, exMessage);
            }
        }
    }
}
