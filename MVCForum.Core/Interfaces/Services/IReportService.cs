using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public interface IReportService
    {
        void MemberReport(Report report);
        void PostReport(Report report);
    }
}
