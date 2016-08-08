using System;
using System.Globalization;
using System.Threading;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using MVCForum.Domain.Events;
using MVCForum.Domain.Interfaces.Services;
using MVCForum.Domain.Interfaces.UnitOfWork;
using MVCForum.IOC;
using MVCForum.Utilities;
using MVCForum.Website.Application;
using MVCForum.Website.Application.ScheduledJobs;
using MVCForum.Website.Application.ViewEngine;

namespace MVCForum.Website
{
    using Domain.Constants;
    using NPoco;

    public class MvcApplication : HttpApplication
    {

        public IUnitOfWorkManager UnitOfWorkManager => ServiceFactory.Get<IUnitOfWorkManager>();
        public IBadgeService BadgeService => ServiceFactory.Get<IBadgeService>();
        public ISettingsService SettingsService => ServiceFactory.Get<ISettingsService>();
        public ILoggingService LoggingService => ServiceFactory.Get<ILoggingService>();
        public ILocalizationService LocalizationService => ServiceFactory.Get<ILocalizationService>();
        public IReflectionService ReflectionService => ServiceFactory.Get<IReflectionService>();


        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            // Start unity
            var unityContainer = UnityHelper.Start();

            // Routes
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            // Store the value for use in the app
            Application["Version"] = AppHelpers.GetCurrentVersionNo();

            // If the same carry on as normal
            LoggingService.Initialise(ConfigUtils.GetAppSettingInt32("LogFileMaxSizeBytes", 10000));
            LoggingService.Error("START APP");

            // Get assemblies for badges, events etc...
            var loadedAssemblies = ReflectionService.GetAssemblies();

            // Do the badge processing
            using (var unitOfWork = UnitOfWorkManager.NewUnitOfWork())
            {
                try
                {
                    BadgeService.SyncBadges(loadedAssemblies);
                    unitOfWork.Commit();
                }
                catch (Exception ex)
                {
                    LoggingService.Error($"Error processing badge classes: {ex.Message}");
                }
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
            var entityContext = HttpContext.Current.Items[SiteConstants.Instance.MvcForumContext] as IDatabase;
            entityContext?.Dispose();
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