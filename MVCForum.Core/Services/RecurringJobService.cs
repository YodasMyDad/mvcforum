namespace MvcForum.Core.Services
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
    using Hangfire;
    using Interfaces;
    using Interfaces.Services;
    using Models;

    /// <summary>
    /// A class that deals with recurring jobs and are all called by hangfire
    /// </summary>
    public partial class RecurringJobService
    {
        private readonly ILoggingService _loggingService;
        private readonly ISettingsService _settingsService;
        private readonly ITopicService _topicService;
        private readonly ILocalizationService _localizationService;
        private readonly IMvcForumContext _context;
        private readonly EmailService _emailService;

        public RecurringJobService(ILoggingService loggingService, ISettingsService settingsService, ITopicService topicService, ILocalizationService localizationService, IMvcForumContext context, EmailService emailService)
        {
            _loggingService = loggingService;
            _settingsService = settingsService;
            _topicService = topicService;
            _localizationService = localizationService;
            _context = context;
            _emailService = emailService;
        }

        /// <inheritdoc />
        public async Task<int> SaveChanges()
        {
            return await _context.SaveChangesAsync();
        }

        /// <summary>
        /// Sends the mark as solution reminder emails
        /// </summary>
        [AutomaticRetry(Attempts = 0)]
        public void SendMarkAsSolutionReminders()
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

                        var emailListToSend = new List<Email>();

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

                            // Add to list
                            emailListToSend.Add(email);

                            // And now mark the topic as reminder sent
                            markAsSolutionReminder.Topic.SolvedReminderSent = true;
                        }

                        // and now pass the emails in to be sent
                        // We have to pass the current settings to SendMail when it's within a task
                        _emailService.SendMail(emailListToSend, settings);

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