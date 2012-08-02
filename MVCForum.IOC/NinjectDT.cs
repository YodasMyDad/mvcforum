using NHibernate;
using Ninject;

namespace MVCForum.IOC
{
    public static class NinjectDT
    {

        public static IKernel Kernel { get; set; }

        /// <summary>
        /// Starts the application
        /// </summary>
        public static void Start()
        {
            CreateKernel();
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            // ??
        }

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            Kernel = new StandardKernel();
            RegisterServices(Kernel);
            return Kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            // NHibernate session factory
            kernel.Bind<ISessionFactory>().ToProvider<NHibernateSessionFactoryProvider>().InSingletonScope();
            kernel.Bind<ISession>().ToMethod(context => context.Kernel.Get<ISessionFactory>().OpenSession());

        }
    }
}
