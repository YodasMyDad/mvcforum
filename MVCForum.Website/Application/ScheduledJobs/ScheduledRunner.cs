using Microsoft.Practices.Unity;
using MVCForum.Domain.Constants;
using Quartz;

namespace MVCForum.Website.Application.ScheduledJobs
{
    public static class ScheduledRunner
    {
        public static void Run(IUnityContainer container)
        {
            // Resolving IScheduler instance
            var scheduler = container.Resolve<IScheduler>();

            #region Triggers

            var fiveMinuteTriggerForever = (ISimpleTrigger)TriggerBuilder.Create()
                 .WithIdentity("FiveMinuteTriggerForever", AppConstants.DefaultTaskGroup)
                 .StartNow()
                 .WithSimpleSchedule(x => x
                     .WithIntervalInMinutes(5)
                     .RepeatForever())
                 .Build();

            var twoMinuteTriggerForever = (ISimpleTrigger)TriggerBuilder.Create()
             .WithIdentity("TwoMinuteTriggerForever", AppConstants.DefaultTaskGroup)
             .StartNow()
             .WithSimpleSchedule(x => x
                 .WithIntervalInMinutes(2)
                 .RepeatForever())
             .Build();

            var fifteenSecondsTriggerForever = (ISimpleTrigger)TriggerBuilder.Create()
                    .WithIdentity("FifteenSecondsTriggerForever", AppConstants.DefaultTaskGroup)
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInSeconds(15)
                        .RepeatForever())
                    .Build();

            var sixHourTriggerForever = (ISimpleTrigger)TriggerBuilder.Create()
                    .WithIdentity("SixHourTriggerForever", AppConstants.DefaultTaskGroup)
                    .StartNow()
                    .WithSimpleSchedule(x => x
                        .WithIntervalInHours(6)
                        .RepeatForever())
                    .Build();

            #endregion

            #region Send Email Job

            // Send emails every 15 seconds

            var emailJob = JobBuilder.Create<EmailJob>()
            .WithIdentity("EmailJob", AppConstants.DefaultTaskGroup)
            .Build();

            scheduler.ScheduleJob(emailJob, fifteenSecondsTriggerForever);

            #endregion

            #region KeepAliveJob

            // Ping the site every 5 minutes to keep it alive
            var keepAliveJob = JobBuilder.Create<KeepAliveJob>()
                        .WithIdentity("KeepAliveJob", AppConstants.DefaultTaskGroup)
                        .Build();

            scheduler.ScheduleJob(keepAliveJob, fiveMinuteTriggerForever);

            #endregion

            #region Mark As Solution Job

            // Send mark as solution reminder emails

            var markAsSolutionReminderJob = JobBuilder.Create<MarkAsSolutionReminderJob>()
            .WithIdentity("MarkAsSolutionReminderJob", AppConstants.DefaultTaskGroup)
            .Build();

            scheduler.ScheduleJob(markAsSolutionReminderJob, sixHourTriggerForever);

            #endregion

            // Starting scheduler
            scheduler.Start();
        }
    }
}