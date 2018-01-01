namespace MvcForum.Web.Application.ScheduledJobs
{
    using System;
    using System.Linq;
    using System.Text;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Core.Models.Entities;
    using Quartz;

    [DisallowConcurrentExecution]
    public class MarkAsSolutionReminderJob : IJob
    {
        private readonly IEmailService _emailService;
        private readonly ILocalizationService _localizationService;
        private readonly ILoggingService _loggingService;
        private readonly ISettingsService _settingsService;
        private readonly ITopicService _topicService;
        private readonly IMvcForumContext _context;

        public MarkAsSolutionReminderJob(ILoggingService loggingService, IEmailService emailService,
            IMvcForumContext context, ITopicService topicService, ISettingsService settingsService,
            ILocalizationService localizationService)
        {
            _loggingService = loggingService;
            _emailService = emailService;
            _context = context;
            _topicService = topicService;
            _settingsService = settingsService;
            _localizationService = localizationService;
        }

        public void Execute(IJobExecutionContext context)
        {

                try
                {
                    var settings = _settingsService.GetSettings(false);
                    var timeFrame = settings.MarkAsSolutionReminderTimeFrame ?? 0;
                    if (timeFrame > 0 && settings.EnableMarkAsSolution)
                    {
                        var remindersToSend = _topicService.GetMarkAsSolutionReminderList(timeFrame);
                        if (remindersToSend.Any())
                        {
                            var amount = remindersToSend.Count;

                            // Use settings days amount and also mark topics as reminded
                            // Only send if markassolution is enabled and day is not 0

                            foreach (var markAsSolutionReminder in remindersToSend)
                            {
                                // Topic Link
                                var topicLink =
                                    $"<a href=\"{settings.ForumUrl.TrimEnd('/')}{markAsSolutionReminder.Topic.NiceUrl}\">{markAsSolutionReminder.Topic.Name}</a>";

                                // Create the email
                                var sb = new StringBuilder();
                                sb.AppendFormat("<p>{0}</p>", string.Format(
                                    _localizationService.GetResourceString("Tasks.MarkAsSolutionReminderJob.EmailBody"),
                                    topicLink,
                                    settings.ForumName, markAsSolutionReminder.PostCount));

                                // create the emails and only send them to people who have not had notifications disabled

                                var user = markAsSolutionReminder.Topic.User;

                                var email = new Email
                                {
                                    Body = _emailService.EmailTemplate(user.UserName, sb.ToString(), settings),
                                    EmailTo = user.Email,
                                    NameTo = user.UserName,
                                    Subject = string.Format(
                                        _localizationService.GetResourceString(
                                            "Tasks.MarkAsSolutionReminderJob.Subject"),
                                        settings.ForumName)
                                };


                                // and now pass the emails in to be sent
                                // We have to pass the current settings to SendMail when it's within a task
                                _emailService.SendMail(email, settings);

                                // And now mark the topic as reminder sent
                                markAsSolutionReminder.Topic.SolvedReminderSent = true;
                            }

                            _context.SaveChanges();
                            _loggingService.Error($"{amount} Mark as solution reminder emails sent");
                        }
                    }
                }
                catch (Exception ex)
                {
                    _context.RollBack();
                    _loggingService.Error(ex);
                }
      
        }
    }
}