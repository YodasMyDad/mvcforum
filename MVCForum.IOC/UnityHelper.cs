using System.Web.Mvc;
using MVCForum.Data.Context;
using MVCForum.Data.Repositories;
using MVCForum.Data.UnitOfWork;
using MVCForum.Domain.Interfaces;
using MVCForum.Domain.Interfaces.Repositories;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.Services;
using Microsoft.Practices.Unity;
using Quartz.Unity;

namespace MVCForum.IOC
{
    // http://weblogs.asp.net/shijuvarghese/archive/2010/05/07/dependency-injection-in-asp-net-mvc-nerddinner-app-using-unity-2-0.aspx

    /// <summary>
    /// Bind the given interface in request scope
    /// </summary>
    public static class IocExtensions
    {
        public static void BindInRequestScope<T1, T2>(this IUnityContainer container) where T2 : T1
        {
            container.RegisterType<T1, T2>(new HierarchicalLifetimeManager());
        }

        public static void BindInSingletonScope<T1, T2>(this IUnityContainer container) where T2 : T1
        {
            container.RegisterType<T1, T2>(new ContainerControlledLifetimeManager());
        }
    }

    /// <summary>
    /// The injection for Unity
    /// </summary>
    public static partial class UnityHelper
    {

        public static IUnityContainer Start()
        {
            var container = new UnityContainer();
            var buildUnity = BuildUnityContainer(container);

            DependencyResolver.SetResolver(new Unity.Mvc4.UnityDependencyResolver(buildUnity));

            return buildUnity;
        }

        /// <summary>
        /// Inject
        /// </summary>
        /// <returns></returns>
        private static IUnityContainer BuildUnityContainer(UnityContainer container)
        {
            

            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // Database context, one per request, ensure it is disposed
            container.BindInRequestScope<IMVCForumContext, MVCForumContext>();
            container.BindInRequestScope<IUnitOfWorkManager, UnitOfWorkManager>();

            // Quartz
            container.AddNewExtension<QuartzUnityExtension>();

            //Bind the various domain model services and repositories that e.g. our controllers require         
            container.BindInRequestScope<IUnitOfWorkManager, UnitOfWorkManager>();
            container.BindInRequestScope<IIntegrityServiceManager, IntegrityServiceManager>();
            container.BindInRequestScope<IRoleService, RoleService>();
            container.BindInRequestScope<ICategoryService, CategoryService>();
            container.BindInRequestScope<IMembershipService, MembershipService>();
            container.BindInRequestScope<IPermissionService, PermissionService>();
            container.BindInRequestScope<ISettingsService, SettingsService>();
            container.BindInRequestScope<ITopicService, TopicService>();
            container.BindInRequestScope<ITopicTagService, TopicTagService>();
            container.BindInRequestScope<IPostService, PostService>();
            container.BindInRequestScope<ILocalizationService, LocalizationService>();
            container.BindInRequestScope<IVoteService, VoteService>();
            container.BindInRequestScope<IBadgeService, BadgeService>();
            container.BindInRequestScope<IMembershipUserPointsService, MembershipUserPointsService>();
            container.BindInRequestScope<ICategoryPermissionForRoleService, CategoryPermissionForRoleService>();
            container.BindInRequestScope<ICategoryNotificationService, CategoryNotificationService>();
            container.BindInRequestScope<ITopicNotificationService, TopicNotificationService>();
            container.BindInRequestScope<IPrivateMessageService, PrivateMessageService>();
            container.BindInRequestScope<ILoggingService, LoggingService>();
            container.BindInRequestScope<IEmailService, EmailService>();
            container.BindInRequestScope<IReportService, ReportService>();
            container.BindInRequestScope<IActivityService, ActivityService>();
            container.BindInRequestScope<IPollService, PollService>();
            container.BindInRequestScope<IPollVoteService, PollVoteService>();
            container.BindInRequestScope<IPollAnswerService, PollAnswerService>();
            container.BindInRequestScope<IInstallerService, InstallerService>();
            container.BindInRequestScope<IBannedEmailService, BannedEmailService>();
            container.BindInRequestScope<IBannedWordService, BannedWordService>();
            container.BindInRequestScope<IUploadedFileService, UploadedFileService>();
            container.BindInRequestScope<IFavouriteService, FavouriteService>();
            container.BindInRequestScope<IGlobalPermissionForRoleService, GlobalPermissionForRoleService>();
            container.BindInRequestScope<ICacheService, CacheService>();
            container.BindInRequestScope<ITagNotificationService, TagNotificationService>();
            container.BindInRequestScope<IReflectionService, ReflectionService>();
            container.BindInRequestScope<IBlockService, BlockService>();

            container.BindInRequestScope<IRoleRepository, RoleRepository>();
            container.BindInRequestScope<ICategoryRepository, CategoryRepository>();
            container.BindInRequestScope<IMembershipRepository, MembershipRepository>();
            container.BindInRequestScope<IPermissionRepository, PermissionRepository>();
            container.BindInRequestScope<ISettingsRepository, SettingsRepository>();
            container.BindInRequestScope<ITopicRepository, TopicRepository>();
            container.BindInRequestScope<ITopicTagRepository, TopicTagRepository>();
            container.BindInRequestScope<IPostRepository, PostRepository>();
            container.BindInRequestScope<ILocalizationRepository, LocalizationRepository>();
            container.BindInRequestScope<IVoteRepository, VoteRepository>();
            container.BindInRequestScope<IBadgeRepository, BadgeRepository>();
            container.BindInRequestScope<IMembershipUserPointsRepository, MembershipUserPointsRepository>();
            container.BindInRequestScope<ICategoryPermissionForRoleRepository, CategoryPermissionForRoleRepository>();
            container.BindInRequestScope<ICategoryNotificationRepository, CategoryNotificationRepository>();
            container.BindInRequestScope<ITopicNotificationRepository, TopicNotificationRepository>();
            container.BindInRequestScope<IPrivateMessageRepository, PrivateMessageRepository>();
            container.BindInRequestScope<IActivityRepository, ActivityRepository>();
            container.BindInRequestScope<IPollRepository, PollRepository>();
            container.BindInRequestScope<IPollVoteRepository, PollVoteRepository>();
            container.BindInRequestScope<IPollAnswerRepository, PollAnswerRepository>();
            container.BindInRequestScope<IInstallerRepository, InstallerRepository>();
            container.BindInRequestScope<IBannedEmailRepository, BannedEmailRepository>();
            container.BindInRequestScope<IBannedWordRepository, BannedWordRepository>();
            container.BindInRequestScope<IUploadedFileRepository, UploadedFileRepository>();
            container.BindInRequestScope<IFavouriteRepository, FavouriteRepository>();
            container.BindInRequestScope<IGlobalPermissionForRoleRepository, GlobalPermissionForRoleRepository>();
            container.BindInRequestScope<IEmailRepository, EmailRepository>();
            container.BindInRequestScope<ITagNotificationRepository, TagNotificationRepository>();
            container.BindInRequestScope<IBlockRepository, BlockRepository>();

            CustomBindings(container);

            return container;
        }

        static partial void CustomBindings(UnityContainer container);
    }

    //public static partial class UnityHelper
    //{
    //    static partial void CustomBindings(UnityContainer container)
    //    {
    //        container.BindInRequestScope<IBlockRepository, BlockRepository>();
    //        container.BindInRequestScope<IBlockService, BlockService>();
    //    }
    //}
}