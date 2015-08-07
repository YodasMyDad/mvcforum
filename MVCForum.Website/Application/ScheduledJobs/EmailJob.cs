using System;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Website.Application;
using Quartz;

namespace MVCForum.Website.ScheduledJobs
{
    public class EmailJob : IJob
    {
        private readonly ILoggingService _loggingService;
        private readonly IEmailService _emailService;
        private readonly IUnitOfWorkManager _unitOfWorkManager;

        public EmailJob(ILoggingService loggingService, IEmailService emailService, IUnitOfWorkManager unitOfWorkManager)
        {
            _loggingService = loggingService;
            _emailService = emailService;
            _unitOfWorkManager = unitOfWorkManager;
        }

        public void Execute(IJobExecutionContext context)
        {
            using (var unitOfWork = _unitOfWorkManager.NewUnitOfWork())
            {
                // Process emails to send - Only send the amount per job from the siteconstants
                _emailService.ProcessMail(SiteConstants.EmailsToSendPerJob);

                try
                {
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