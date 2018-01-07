namespace MvcForum.Core.Ioc
{
    using System.Web.Mvc;
    using Data.Context;
    using Interfaces;
    using Interfaces.Services;
    using Services;
    using Unity;
    using Unity.Lifetime;

    /// <summary>
    ///     Bind the given interface in request scope
    /// </summary>
    public static class IocExtensions
    {
        public static void BindInRequestScope<T1, T2>(this IUnityContainer container) where T2 : T1
        {
            container.RegisterType<T1, T2>(new HierarchicalLifetimeManager());
        }
    }

    /// <summary>
    ///     The injection for Unity
    /// </summary>
    public static class UnityHelper
    {
        public static IUnityContainer Start()
        {
            var container = new UnityContainer();
            DependencyResolver.SetResolver(new UnityDependencyResolver(container));
            var buildUnity = BuildUnityContainer(container);
            return buildUnity;
        }

        /// <summary>
        ///     Inject
        /// </summary>
        /// <returns></returns>
        private static IUnityContainer BuildUnityContainer(UnityContainer container)
        {
            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // Database context, one per request, ensure it is disposed
            container.BindInRequestScope<IMvcForumContext, MvcForumContext>();

            //Bind the various domain model services and repositories that e.g. our controllers require         
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
            container.BindInRequestScope<IBannedEmailService, BannedEmailService>();
            container.BindInRequestScope<IBannedWordService, BannedWordService>();
            container.BindInRequestScope<IUploadedFileService, UploadedFileService>();
            container.BindInRequestScope<IFavouriteService, FavouriteService>();
            container.BindInRequestScope<IGlobalPermissionForRoleService, GlobalPermissionForRoleService>();
            container.BindInRequestScope<ICacheService, CacheService>();
            container.BindInRequestScope<ITagNotificationService, TagNotificationService>();
            container.BindInRequestScope<IReflectionService, ReflectionService>();
            container.BindInRequestScope<IBlockService, BlockService>();
            container.BindInRequestScope<IConfigService, ConfigService>();
            container.BindInRequestScope<IPostEditService, PostEditService>();

            return container;
        }
    }

    // Example of adding your own bindings, just create a partial class and implement
    // the CustomBindings method and add your bindings as shown below
    //public static partial class UnityHelper
    //{
    //    static partial void CustomBindings(UnityContainer container)
    //    {
    //        container.BindInRequestScope<IBlockRepository, BlockRepository>();
    //        container.BindInRequestScope<IBlockService, BlockService>();
    //    }
    //}
}