using Quartz;
using Quartz.Core;
using Quartz.Impl;

namespace MVCForum.IOC.Quartz
{
    public class UnitySchedulerFactory : StdSchedulerFactory
    {
        private readonly UnityJobFactory _unityJobFactory;

        public UnitySchedulerFactory(UnityJobFactory unityJobFactory)
        {
            this._unityJobFactory = unityJobFactory;
        }

        protected override IScheduler Instantiate(QuartzSchedulerResources rsrcs, QuartzScheduler qs)
        {
            qs.JobFactory = this._unityJobFactory;
            return base.Instantiate(rsrcs, qs);
        }
    }
}
