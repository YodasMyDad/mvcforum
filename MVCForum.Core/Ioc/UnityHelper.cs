namespace MvcForum.Core.Ioc
{
    using System.Web.Mvc;
    using Data.Context;
    using Interfaces;
    using Interfaces.Services;
    using Reflection;
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

        public static IUnityContainer Container;

        public static void InitialiseUnityContainer()
        {
            Container = new UnityContainer();
            DependencyResolver.SetResolver(new UnityDependencyResolver(Container));

            // Bit annoying having just this here but we need this early in the startup for seed method
            Container.BindInRequestScope<IConfigService, ConfigService>();
            Container.BindInRequestScope<ICacheService, CacheService>();
        }


        /// <summary>
        ///     Inject
        /// </summary>
        /// <returns></returns>
        public static void BuildUnityContainer()
        {
            // register all your components with the container here
            // it is NOT necessary to register your controllers

            // Database context, one per request, ensure it is disposed
            Container.BindInRequestScope<IMvcForumContext, MvcForumContext>();

            //Bind the various domain model services and repositories that e.g. our controllers require         
            Container.BindInRequestScope<IRoleService, RoleService>();
            Container.BindInRequestScope<ICategoryService, CategoryService>();
            Container.BindInRequestScope<IMembershipService, MembershipService>();
            Container.BindInRequestScope<IPermissionService, PermissionService>();
            Container.BindInRequestScope<ISettingsService, SettingsService>();
            Container.BindInRequestScope<ITopicService, TopicService>();
            Container.BindInRequestScope<ITopicTagService, TopicTagService>();
            Container.BindInRequestScope<IPostService, PostService>();
            Container.BindInRequestScope<ILocalizationService, LocalizationService>();
            Container.BindInRequestScope<IVoteService, VoteService>();
            Container.BindInRequestScope<IBadgeService, BadgeService>();
            Container.BindInRequestScope<IMembershipUserPointsService, MembershipUserPointsService>();
            Container.BindInRequestScope<ICategoryPermissionForRoleService, CategoryPermissionForRoleService>();
            Container.BindInRequestScope<IPrivateMessageService, PrivateMessageService>();
            Container.BindInRequestScope<ILoggingService, LoggingService>();
            Container.BindInRequestScope<IEmailService, EmailService>();
            Container.BindInRequestScope<IReportService, ReportService>();
            Container.BindInRequestScope<IActivityService, ActivityService>();
            Container.BindInRequestScope<IPollService, PollService>();
            Container.BindInRequestScope<IBannedEmailService, BannedEmailService>();
            Container.BindInRequestScope<IBannedWordService, BannedWordService>();
            Container.BindInRequestScope<IUploadedFileService, UploadedFileService>();
            Container.BindInRequestScope<IFavouriteService, FavouriteService>();
            Container.BindInRequestScope<IGlobalPermissionForRoleService, GlobalPermissionForRoleService>();            
            Container.BindInRequestScope<INotificationService, NotificationService>();
            Container.BindInRequestScope<IBlockService, BlockService>();            
            Container.BindInRequestScope<IPostEditService, PostEditService>();
            Container.BindInRequestScope<IAssemblyProvider, AssemblyProvider>();
            Container.BindInRequestScope<ISpamService, SpamService>();
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