using Microsoft.Owin;
using Owin;
using Hangfire;

[assembly: OwinStartup(typeof(MvcForum.Web.Startup))]
namespace MvcForum.Web
{
    using System;
    using System.Data.Entity;
    using System.Web.Mvc;
    using System.Web.Routing;
    using Application.ViewEngine;
    using Core.Constants;
    using Core.Data.Context;
    using Core.Events;
    using Core.Interfaces;
    using Core.Ioc;
    using Core.Services.Migrations;
    using Core.Utilities;
    using Core.Interfaces.Services;
    using Core.Services;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            AreaRegistration.RegisterAllAreas();
            System.Web.Http.GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            // Start unity
            var unityContainer = UnityHelper.Start();

            // Set Hangfire to use SQL Server and the connection string
            GlobalConfiguration.Configuration.UseSqlServerStorage(SiteConstants.Instance.MvcForumContext);

            // Make hangfire use unity container
            GlobalConfiguration.Configuration.UseUnityActivator(unityContainer);

            // Add Hangfire
            // TODO - Do I need this dashboard?
            //app.UseHangfireDashboard();
            app.UseHangfireServer();

            // Make DB update to latest migration
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<MvcForumContext, Configuration>());

            // Get services needed
            var mvcForumContext = DependencyResolver.Current.GetService<IMvcForumContext>();
            var badgeService = DependencyResolver.Current.GetService<IBadgeService>();
            var settingsService = DependencyResolver.Current.GetService<ISettingsService>();
            var loggingService = DependencyResolver.Current.GetService<ILoggingService>();
            var reflectionService = DependencyResolver.Current.GetService<IReflectionService>();

            // Routes
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // If the same carry on as normal
            loggingService.Initialise(ConfigUtils.GetAppSettingInt32("LogFileMaxSizeBytes", 10000));
            loggingService.Error("START APP");

            // Get assemblies for badges, events etc...
            var loadedAssemblies = reflectionService.GetAssemblies();

            // Do the badge processing
            try
            {
                badgeService.SyncBadges(loadedAssemblies);
                mvcForumContext.SaveChanges();
            }
            catch (Exception ex)
            {
                loggingService.Error($"Error processing badge classes: {ex.Message}");
            }

            // Set the view engine
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new ForumViewEngine(settingsService.GetSettings().Theme));

            // Initialise the events
            EventManager.Instance.Initialize(loggingService, loadedAssemblies);

            // Finally trigger any Cron jobs
            RecurringJob.AddOrUpdate<RecurringJobService>(x => x.SendMarkAsSolutionReminders(), Cron.HourInterval(6), queue: "solutionreminders");
        }
    }
}