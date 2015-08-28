using System;
using System.Linq;
using System.Text;
using MVCForum.Domain.DomainModel;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using Quartz;

namespace MVCForum.Website.Application.ScheduledJobs
{
    public class MarkAsSolutionReminderJob : IJob
    {
        private readonly ILoggingService _loggingService;
        private readonly ILocalizationService _localizationService;
        private readonly IEmailService _emailService;
        private readonly ITopicService _topicService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;
        private readonly ISettingsService _settingsService;

        public MarkAsSolutionReminderJob(ILoggingService loggingService, IEmailService emailService, IUnitOfWorkManager unitOfWorkManager, ITopicService topicService, ISettingsService settingsService, ILocalizationService localizationService)
        {
            _loggingService = loggingService;
            _emailService = emailService;
            _unitOfWorkManager = unitOfWorkManager;
            _topicService = topicService;
            _settingsService = settingsService;
            _localizationService = localizationService;
        }

        public void Execute(IJobExecutionContext context)
        {
            using (var unitOfWork = _unitOfWorkManager.NewUnitOfWork())
            {
                var settings = _settingsService.GetSettings(false);
                var timeFrame = settings.MarkAsSolutionReminderTimeFrame ?? 0;
                if (timeFrame > 0 && settings.EnableMarkAsSolution)
                {
                    var amount = 0;
                    try
                    {

                        var remindersToSend = _topicService.GetMarkAsSolutionReminderList(timeFrame);
                        if (remindersToSend.Any())
                        {
                            amount = remindersToSend.Count;

                            // Use settings days amount and also mark topics as reminded
                            // Only send if markassolution is enabled and day is not 0

                            foreach (var markAsSolutionReminder in remindersToSend)
                            {
                                // Topic Link
                                var topicLink = string.Format("<a href=\"{0}{1}\">{2}</a>",
                                    settings.ForumUrl.TrimEnd('/'),
                                    markAsSolutionReminder.Topic.NiceUrl,
                                    markAsSolutionReminder.Topic.Name);

                                // Create the email
                                var sb = new StringBuilder();
                                sb.AppendFormat("<p>{0}</p>", string.Format(_localizationService.GetResourceString("Tasks.MarkAsSolutionReminderJob.EmailBody"),
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
                                        _localizationService.GetResourceString("Tasks.MarkAsSolutionReminderJob.Subject"),
                                        settings.ForumName)
                                };


                                // and now pass the emails in to be sent
                                _emailService.SendMail(email);

                                // And now mark the topic as reminder sent
                                markAsSolutionReminder.Topic.SolvedReminderSent = true;
                            }
                        }


                        unitOfWork.Commit();
                        _loggingService.Error(string.Format("{0} Mark as solution reminder emails sent", amount));
                    }
                    catch (Exception ex)
                    {
                        unitOfWork.Rollback();
                        _loggingService.Error(ex);
                    }
                }
            }
        }
    }
}