using System;
using System.Globalization;
using Common.Logging;
using Microsoft.Practices.Unity;
using Quartz;
using Quartz.Spi;

namespace MVCForum.IOC.Quartz
{
    public class UnityJobFactory : IJobFactory
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(UnityJobFactory));
        private readonly IUnityContainer _container;

        static UnityJobFactory()
        {
        }

        public UnityJobFactory(IUnityContainer container)
        {
            this._container = container;
        }

        public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
        {
            var jobDetail = bundle.JobDetail;
            var jobType = jobDetail.JobType;

            try
            {
                if (Log.IsDebugEnabled)
                {
                    Log.Debug(string.Format(
                        CultureInfo.InvariantCulture,
                        "Producing instance of Job '{0}', class={1}", new object[] { jobDetail.Key, jobType.FullName }));
                }

                return typeof(IInterruptableJob).IsAssignableFrom(jobType)
                    ? new InterruptableJobWrapper(bundle, _container)
                    : new JobWrapper(bundle, _container);
            }
            catch (Exception ex)
            {
                throw new SchedulerException(string.Format(
                    CultureInfo.InvariantCulture,
                    "Problem instantiating class '{0}'", new object[] { jobDetail.JobType.FullName }), ex);
            }
        }

        public void ReturnJob(IJob job)
        {
            // Nothing here. Unity does not maintain a handle to container created instances.
        }


        #region Job Wrappers

        /// <summary>
        ///     Job execution wrapper.
        /// </summary>
        /// <remarks>
        ///     Creates nested lifetime scope per job execution and resolves Job from Autofac.
        /// </remarks>
        internal class JobWrapper : IJob
        {
            private readonly TriggerFiredBundle _bundle;
            private readonly IUnityContainer _unityContainer;

            /// <summary>
            ///     Initializes a new instance of the <see cref="T:System.Object" /> class.
            /// </summary>
            public JobWrapper(TriggerFiredBundle bundle, IUnityContainer unityContainer)
            {
                if (bundle == null)
                {
                    throw new ArgumentNullException("bundle");
                }

                if (unityContainer == null)
                {
                    throw new ArgumentNullException("unityContainer");
                }

                this._bundle = bundle;
                this._unityContainer = unityContainer;
            }

            protected IJob RunningJob { get; private set; }
            
            /// <summary>
            ///     Called by the <see cref="T:Quartz.IScheduler" /> when a <see cref="T:Quartz.ITrigger" />
            ///     fires that is associated with the <see cref="T:Quartz.IJob" />.
            /// </summary>
            /// <remarks>
            ///     The implementation may wish to set a  result object on the
            ///     JobExecutionContext before this method exits.  The result itself
            ///     is meaningless to Quartz, but may be informative to
            ///     <see cref="T:Quartz.IJobListener" />s or
            ///     <see cref="T:Quartz.ITriggerListener" />s that are watching the job's
            ///     execution.
            /// </remarks>
            /// <param name="context">The execution context.</param>
            /// <exception cref="SchedulerConfigException">Job cannot be instantiated.</exception>
            public void Execute(IJobExecutionContext context)
            {
                var childContainer = _unityContainer.CreateChildContainer();
                try
                {
                    RunningJob = (IJob)childContainer.Resolve(_bundle.JobDetail.JobType);
                    RunningJob.Execute(context);
                }
                catch (JobExecutionException)
                {
                    throw;
                }
                catch (Exception ex)
                {
                    throw new JobExecutionException(string.Format(CultureInfo.InvariantCulture,
                        "Failed to execute Job '{0}' of type '{1}'",
                        _bundle.JobDetail.Key, _bundle.JobDetail.JobType), ex);
                }
                finally
                {
                    RunningJob = null;
                    childContainer.Dispose();
                }
            }
        }
        
        internal sealed class InterruptableJobWrapper : JobWrapper, IInterruptableJob
        {
            public InterruptableJobWrapper(TriggerFiredBundle bundle, IUnityContainer unityContainer)
                : base(bundle, unityContainer)
            {
            }

            public void Interrupt()
            {
                var interruptableJob = RunningJob as IInterruptableJob;
                
                if (interruptableJob != null)
                {
                    interruptableJob.Interrupt();
                }
            }
        }

        #endregion
    }
}
