namespace MvcForum.Web.Application.ScheduledJobs
{
    using System;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Quartz;

    [DisallowConcurrentExecution]
    public class EmailJob : IJob
    {
        private readonly IEmailService _emailService;
        private readonly ILoggingService _loggingService;
        private readonly IMvcForumContext _context;

        public EmailJob(ILoggingService loggingService, IEmailService emailService,
            IMvcForumContext context)
        {
            _loggingService = loggingService;
            _emailService = emailService;
            _context = context;
        }

        public void Execute(IJobExecutionContext context)
        {

                try
                {
                    // Process emails to send - Only send the amount per job from the siteconstants
                    _emailService.ProcessMail(5);

                    // Commit - Which deletes the jobs that have been sent
                    _context.SaveChanges();
                }
                catch (Exception ex)
                {
                    _context.RollBack();
                    _loggingService.Error(ex);
                }
         
        }
    }
}