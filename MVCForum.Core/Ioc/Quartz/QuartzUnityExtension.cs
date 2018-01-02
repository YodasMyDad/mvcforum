namespace MvcForum.Core.Ioc.Quartz
{
    using global::Quartz;
    using Unity;
    using Unity.Extension;
    using Unity.Injection;
    using Unity.Lifetime;

    public class QuartzUnityExtension : UnityContainerExtension
    {
        protected override void Initialize()
        {
            Container.RegisterType<ISchedulerFactory, UnitySchedulerFactory>(new ContainerControlledLifetimeManager());
            Container.RegisterType<IScheduler>(new InjectionFactory(c =>
                c.Resolve<ISchedulerFactory>().GetScheduler()));
        }
    }
}