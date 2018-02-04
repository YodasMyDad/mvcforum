namespace MvcForum.Core.Services
{
    using System.Text;
    using System.Threading.Tasks;
    using Interfaces;
    using Interfaces.Services;
    using Models;
    using Models.General;

    public partial class ReportService : IReportService
    {
        private readonly IEmailService _emailService;
        private readonly ISettingsService _settingsService;
        private readonly ILocalizationService _localizationService;

        public ReportService(IEmailService emailService, ISettingsService settingsService, ILocalizationService localizationService)
        {
            _emailService = emailService;
            _settingsService = settingsService;
            _localizationService = localizationService;
        }

        /// <inheritdoc />
        public void RefreshContext(IMvcForumContext context)
        {
            _emailService.RefreshContext(context);
            _settingsService.RefreshContext(context);
            _localizationService.RefreshContext(context);
        }

        /// <inheritdoc />
        public Task<int> SaveChanges()
        {
            throw new System.NotImplementedException();
        }

        /// <summary>
        /// Report a member
        /// </summary>
        /// <param name="report"></param>
        public void MemberReport(Report report)
        {
            var sb = new StringBuilder();
            var email = new Email();

            sb.AppendFormat("<p>{2}: <a href=\"{0}\">{1}</a></p>", 
                string.Concat(_settingsService.GetSettings().ForumUrl.TrimEnd('/'), report.Reporter.NiceUrl), 
                report.Reporter.UserName,
                _localizationService.GetResourceString("Report.Reporter"));

            sb.AppendFormat("<p>{2}: <a href=\"{0}\">{1}</a></p>",
                string.Concat(_settingsService.GetSettings().ForumUrl.TrimEnd('/'), report.ReportedMember.NiceUrl), 
                report.ReportedMember.UserName,
                _localizationService.GetResourceString("Report.MemberReported"));

            sb.Append($"<p>{_localizationService.GetResourceString("Report.Reason")}:</p>");
            sb.Append($"<p>{report.Reason}</p>");

            email.EmailTo = _settingsService.GetSettings().AdminEmailAddress;
            email.Subject = _localizationService.GetResourceString("Report.MemberReport");
            email.NameTo = _localizationService.GetResourceString("Report.Admin");

            email.Body = _emailService.EmailTemplate(email.NameTo, sb.ToString());
            _emailService.SendMail(email);
        }

        /// <summary>
        /// Report a post
        /// </summary>
        /// <param name="report"></param>
        public void PostReport(Report report)
        {
            var sb = new StringBuilder();
            var email = new Email();

            sb.AppendFormat("<p>{2}: <a href=\"{0}\">{1}</a></p>", string.Concat(_settingsService.GetSettings().ForumUrl.TrimEnd('/'), report.Reporter.NiceUrl), 
                report.Reporter.UserName,
                _localizationService.GetResourceString("Report.Reporter"));

            var urlOfPost =$"{_settingsService.GetSettings().ForumUrl.TrimEnd('/')}{report.ReportedPost.Topic.NiceUrl}?order=all#comment-{report.ReportedPost.Id}";
            sb.AppendFormat("<p>{2}: <a href=\"{0}\">{1}</a></p>", urlOfPost, report.ReportedPost.Topic.Name, _localizationService.GetResourceString("Report.PostReported"));

            sb.Append($"<p>{_localizationService.GetResourceString("Report.Reason")}:</p>");
            sb.Append($"<p>{report.Reason}</p>");

            email.EmailTo = _settingsService.GetSettings().AdminEmailAddress;
            email.Subject = _localizationService.GetResourceString("Report.PostReport");
            email.NameTo = _localizationService.GetResourceString("Report.Admin");

            email.Body = _emailService.EmailTemplate(email.NameTo, sb.ToString());

            _emailService.SendMail(email);
        }
    }
}
