﻿using System.Text;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;

namespace MVCForum.Services
{
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

            sb.AppendFormat("<p>{0}:</p>", _localizationService.GetResourceString("Report.Reason"));
            sb.AppendFormat("<p>{0}</p>", report.Reason);

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

            sb.AppendFormat("<p>{0}:</p>", _localizationService.GetResourceString("Report.Reason"));
            sb.AppendFormat("<p>{0}</p>", report.Reason);

            email.EmailTo = _settingsService.GetSettings().AdminEmailAddress;
            email.Subject = _localizationService.GetResourceString("Report.PostReport");
            email.NameTo = _localizationService.GetResourceString("Report.Admin");

            email.Body = _emailService.EmailTemplate(email.NameTo, sb.ToString());

            _emailService.SendMail(email);
        }
    }
}
