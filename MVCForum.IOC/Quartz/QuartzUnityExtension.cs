using Microsoft.Practices.Unity;
using Quartz;

namespace MVCForum.IOC.Quartz
{
    public class QuartzUnityExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {
            this.Container.RegisterType<ISchedulerFactory, UnitySchedulerFactory>(new ContainerControlledLifetimeManager());
            this.Container.RegisterType<IScheduler>(new InjectionFactory(c => c.Resolve<ISchedulerFactory>().GetScheduler()));
        }
    }
}
