namespace MvcForum.Core.Ioc.Quartz
{
    using global::Quartz;
    using global::Quartz.Core;
    using global::Quartz.Impl;

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
