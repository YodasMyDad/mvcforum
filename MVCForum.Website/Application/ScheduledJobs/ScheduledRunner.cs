using Microsoft.Practices.Unity;
using Quartz;
using Quartz.Impl;
using Quartz.Impl.Triggers;

namespace MVCForum.Website.Application.ScheduledJobs
{
    public static class ScheduledRunner
    {
        public static void Run(IUnityContainer container)
        {
            // Resolving IScheduler instance
            var scheduler = container.Resolve<IScheduler>();

            // Scheduling jobs

            // Send emails every 15 seconds
            scheduler.ScheduleJob(
                new JobDetailImpl("EmailJob", typeof(EmailJob)),
                new CalendarIntervalTriggerImpl("EmailJobTrigger", IntervalUnit.Second, 15));

            // Ping the site every 5 minutes to keep it alive
            scheduler.ScheduleJob(
                new JobDetailImpl("KeepAliveJob", typeof(KeepAliveJob)),
                new CalendarIntervalTriggerImpl("KeepAliveTrigger", IntervalUnit.Minute, 5));

            // Send mark as solution reminder emails
            scheduler.ScheduleJob(
                new JobDetailImpl("MarkAsSolutionReminderJob", typeof(MarkAsSolutionReminderJob)),
                new CalendarIntervalTriggerImpl("MarkAsSolutionReminderJobTrigger", IntervalUnit.Hour, 6));

            // Starting scheduler
            scheduler.Start();
        }
    }
}