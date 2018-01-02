namespace MvcForum.Web
{
    using System;
    using System.Data.Entity;
    using System.Globalization;
    using System.Threading;
    using System.Web;
    using System.Web.Http;
    using System.Web.Mvc;
    using System.Web.Routing;
    using Application;
    using Application.ScheduledJobs;
    using Application.ViewEngine;
    using Core.Data.Context;
    using Core.Events;
    using Core.Interfaces;
    using Core.Interfaces.Services;
    using Core.Ioc;
    using Core.Services.Migrations;
    using Core.Utilities;

    public class MvcApplication : HttpApplication
    {
        public IMvcForumContext MvcForumContext => DependencyResolver.Current.GetService<IMvcForumContext>();
        public IBadgeService BadgeService => DependencyResolver.Current.GetService<IBadgeService>();
        public ISettingsService SettingsService => DependencyResolver.Current.GetService<ISettingsService>();
        public ILoggingService LoggingService => DependencyResolver.Current.GetService<ILoggingService>();

        public ILocalizationService LocalizationService =>
            DependencyResolver.Current.GetService<ILocalizationService>();

        public IReflectionService ReflectionService => DependencyResolver.Current.GetService<IReflectionService>();


        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            // Start unity
            var unityContainer = UnityHelper.Start();

            // Routes
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // Store the value for use in the app
            Application["Version"] = AppHelpers.GetCurrentVersionNo();

            // Make DB update to latest migration
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<MvcForumContext, Configuration>());

            // If the same carry on as normal
            LoggingService.Initialise(ConfigUtils.GetAppSettingInt32("LogFileMaxSizeBytes", 10000));
            LoggingService.Error("START APP");

            // Get assemblies for badges, events etc...
            var loadedAssemblies = ReflectionService.GetAssemblies();

            // Do the badge processing

            try
            {
                BadgeService.SyncBadges(loadedAssemblies);
                MvcForumContext.SaveChanges();
            }
            catch (Exception ex)
            {
                LoggingService.Error($"Error processing badge classes: {ex.Message}");
            }


            // Set the view engine
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new ForumViewEngine(SettingsService.GetSettings().Theme));

            // Initialise the events
            EventManager.Instance.Initialize(LoggingService, loadedAssemblies);

            // Finally Run scheduled tasks
            ScheduledRunner.Run(unityContainer);
        }

        protected void Application_AcquireRequestState(object sender, EventArgs e)
        {
            //It's important to check whether session object is ready
            if (HttpContext.Current.Session != null)
            {
                // Set the culture per request
                var ci = new CultureInfo(LocalizationService.CurrentLanguage.LanguageCulture);
                Thread.CurrentThread.CurrentUICulture = ci;
                Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(ci.Name);
            }
        }

        protected void Application_EndRequest(object sender, EventArgs e)
        {
        }

        protected void Application_Error(object sender, EventArgs e)
        {
            var lastError = Server.GetLastError();
            // Don't flag missing pages or changed urls, as just clogs up the log
            if (!lastError.Message.Contains("was not found or does not implement IController"))
            {
                LoggingService.Error(lastError);
            }
        }
    }
}