namespace MvcForum.Core.Interfaces.Services
{
    using DomainModel.General;

    public partial interface IReportService
    {
        void MemberReport(Report report);
        void PostReport(Report report);
    }
}