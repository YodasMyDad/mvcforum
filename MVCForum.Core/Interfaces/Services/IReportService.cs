using MVCForum.Domain.DomainModel;

namespace MVCForum.Domain.Interfaces.Services
{
    public partial interface IReportService
    {
        void MemberReport(Report report);
        void PostReport(Report report);
    }
}
