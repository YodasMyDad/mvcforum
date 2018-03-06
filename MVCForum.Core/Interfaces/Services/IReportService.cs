namespace MvcForum.Core.Interfaces.Services
{
    using Models.General;

    public partial interface IReportService : IContextService
    {
        void MemberReport(Report report);
        void PostReport(Report report);
    }
}