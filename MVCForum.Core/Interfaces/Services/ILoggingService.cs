using System;
using System.Collections.Generic;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public partial interface ILoggingService
    {
        void Error(string message);
        void Error(Exception ex);
        void Initialise(int maxLogSize);
        IList<LogEntry> ListLogFile();
        void Recycle();
        void ClearLogFiles();
    }
}
