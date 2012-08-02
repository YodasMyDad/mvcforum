using System;
using System.Collections.Generic;
using System.Web;
using MVCForum.API;
using MVCForum.Cache;
using MVCForum.Data.Repositories;
using MVCForum.Data.Session;
using MVCForum.Data.UnitOfWork;
using MVCForum.Domain.Interfaces.API;
using Ninject;
using Ninject.Web.Common;
using NHibernate;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Services;
using Microsoft.Web.Infrastructure.DynamicModuleHelper;

namespace MVCForum.IOC
{
    public static class NinjectMVC3
    {
        /// <summary>
        /// This is a general object that the caller can pass. If not null it will be
        /// processed by Ninject after bindings have occurred
        /// </summary>
        private static List<object> _objectsToInject;

        private static readonly Bootstrapper Bootstrapper = new Bootstrapper();

        /// <summary>
        /// Starts the web application
        /// </summary>
        public static void Start()
        {
            DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
            DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));       
        }

        public static void StartInjection(List<object> objsToInject)
        {
            _objectsToInject = objsToInject;
            Bootstrapper.Initialize(CreateKernel);
        }

        /// <summary>
        /// Stops the application.
        /// </summary>
        public static void Stop()
        {
            Bootstrapper.ShutDown();
        }

        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        private static IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            RegisterServices(kernel);
            return kernel;
        }

        /// <summary>
        /// Load your modules or register your services here!
        /// </summary>
        /// <param name="kernel">The kernel.</param>
        private static void RegisterServices(IKernel kernel)
        {
            // http://stackoverflow.com/questions/9693957/ninject-web-common-throwing-activationexception-trying-to-inject-dependencies-in
            kernel.Bind<Func<IKernel>>().ToMethod(ctx => () => new Bootstrapper().Kernel);
            kernel.Bind<IHttpModule>().To<HttpApplicationInitializationHttpModule>();

            // NHibernate session factory
            kernel.Bind<ISessionFactory>().ToProvider<NHibernateSessionFactoryProvider>().InSingletonScope();
            kernel.Bind<ISession>().ToMethod(context => context.Kernel.Get<ISessionFactory>().OpenSession()).InRequestScope();
            kernel.Bind<IIntegrityServiceManager>().To<IntegrityServiceManager>().InRequestScope();
            kernel.Bind<IUnitOfWorkManager>().To<UnitOfWorkManager>().InRequestScope();

            // Bind the various domain model services and repositories that e.g. our controllers require         
            kernel.Bind<IRoleService>().To<RoleService>().InRequestScope();
            kernel.Bind<ICategoryService>().To<CategoryService>().InRequestScope();
            kernel.Bind<IMembershipService>().To<MembershipService>().InRequestScope();
            kernel.Bind<IPermissionService>().To<PermissionService>().InRequestScope();
            kernel.Bind<ISettingsService>().To<SettingsService>().InRequestScope();
            kernel.Bind<ITopicService>().To<TopicService>().InRequestScope();
            kernel.Bind<ITopicTagService>().To<TopicTagService>().InRequestScope();
            kernel.Bind<IPostService>().To<PostService>().InRequestScope();
            kernel.Bind<ILocalizationService>().To<LocalizationService>().InRequestScope();
            kernel.Bind<IVoteService>().To<VoteService>().InRequestScope();
            kernel.Bind<IBadgeService>().To<BadgeService>().InRequestScope();
            kernel.Bind<IMembershipUserPointsService>().To<MembershipUserPointsService>().InRequestScope();
            kernel.Bind<ICategoryPermissionForRoleService>().To<CategoryPermissionForRoleService>().InRequestScope();
            kernel.Bind<ICategoryNotificationService>().To<CategoryNotificationService>().InRequestScope();
            kernel.Bind<ITopicNotificationService>().To<TopicNotificationService>().InRequestScope();
            kernel.Bind<IPrivateMessageService>().To<PrivateMessageService>().InRequestScope();
            kernel.Bind<ILoggingService>().To<LoggingService>().InRequestScope();
            kernel.Bind<IEmailService>().To<EmailService>().InRequestScope();
            kernel.Bind<IReportService>().To<ReportService>().InRequestScope();
            kernel.Bind<IActivityService>().To<ActivityService>().InRequestScope();

            kernel.Bind<IRoleRepository>().To<RoleRepository>().InRequestScope();
            kernel.Bind<ICategoryRepository>().To<CategoryRepository>().InRequestScope();
            kernel.Bind<IMembershipRepository>().To<MembershipRepository>().InRequestScope();
            kernel.Bind<IPermissionRepository>().To<PermissionRepository>().InRequestScope();
            kernel.Bind<ISettingsRepository>().To<SettingsRepository>().InRequestScope();
            kernel.Bind<ITopicRepository>().To<TopicRepository>().InRequestScope();
            kernel.Bind<ITopicTagRepository>().To<TopicTagRepository>().InRequestScope();
            kernel.Bind<IPostRepository>().To<PostRepository>().InRequestScope();
            kernel.Bind<ILocalizationRepository>().To<LocalizationRepository>().InRequestScope();
            kernel.Bind<IVoteRepository>().To<VoteRepository>().InRequestScope();
            kernel.Bind<IBadgeRepository>().To<BadgeRepository>().InRequestScope();
            kernel.Bind<IMembershipUserPointsRepository>().To<MembershipUserPointsRepository>().InRequestScope();
            kernel.Bind<ICategoryPermissionForRoleRepository>().To<CategoryPermissionForRoleRepository>().InRequestScope();                        
            kernel.Bind<ICategoryNotificationRepository>().To<CategoryNotificationRepository>().InRequestScope();                                   
            kernel.Bind<ITopicNotificationRepository>().To<TopicNotificationRepository>().InRequestScope();
            kernel.Bind<IPrivateMessageRepository>().To<PrivateMessageRepository>().InRequestScope();
            kernel.Bind<IActivityRepository>().To<ActivityRepository>().InRequestScope();

            kernel.Bind<IMVCForumAPI>().To<MVCForumAPI>().InRequestScope();
            kernel.Bind<IPostAPI>().To<PostAPI>().InRequestScope();
            kernel.Bind<ITopicAPI>().To<TopicAPI>().InRequestScope();

            kernel.Bind<ICacheHelper>().To<CacheHelper>().InRequestScope();
            kernel.Bind<ISessionHelper>().To<SessionHelper>().InRequestScope();

            // Locate the membership objects defined via web.config and inject thereby setting any properties decorated with [Inject]
            // applying any ninjectified objects already established
            kernel.Inject(System.Web.Security.Membership.Provider);
            kernel.Inject(System.Web.Security.Roles.Provider);

            // Inject the objects passed by caller e.g. ServiceLocator
            foreach(var obj in _objectsToInject)
            {
                if (obj != null)
                {
                    kernel.Inject(obj);
                }
            }
        }
    }
}
