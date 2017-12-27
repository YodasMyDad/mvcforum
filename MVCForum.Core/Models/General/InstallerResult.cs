namespace MvcForum.Core.DomainModel.General
{
    using System;

    public partial class InstallerResult
    {
        public string Message { get; set; }
        public bool Successful { get; set; }
        public Exception Exception { get; set; }

        public string OnScreenMessage
        {
            get
            {
                var exMessage = Exception != null
                    ? $"<br /><strong>Error Message:<strong> {Exception.Message}<br />{Exception.InnerException}"
                    : string.Empty;
                return $"<p><strong>{Message}<strong>{exMessage}</p>";
            }
        }
    }
}