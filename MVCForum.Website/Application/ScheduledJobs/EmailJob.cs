namespace MvcForum.Web.Application.ScheduledJobs
{
    using System;
    using Core.Interfaces.Services;
    using Core.Interfaces.UnitOfWork;
    using Quartz;

    [DisallowConcurrentExecution]
    public class EmailJob : IJob
    {
        private readonly IEmailService _emailService;
        private readonly ILoggingService _loggingService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public EmailJob(ILoggingService loggingService, IEmailService emailService,
            IUnitOfWorkManager unitOfWorkManager)
        {
            _loggingService = loggingService;
            _emailService = emailService;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public void Execute(IJobExecutionContext context)
        {
            using (var unitOfWork = _unitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    // Process emails to send - Only send the amount per job from the siteconstants
                    _emailService.ProcessMail(5);

                    // Commit - Which deletes the jobs that have been sent
                    unitOfWork.Commit();
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