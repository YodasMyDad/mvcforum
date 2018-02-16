using Microsoft.Owin;
using Owin;
using Hangfire;

[assembly: OwinStartup(typeof(MvcForum.Web.Startup))]
namespace MvcForum.Web
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Web.Mvc;
    using System.Web.Routing;
    using Application.ViewEngine;
    using Core;
    using Core.Data.Context;
    using Core.Events;
    using Core.Interfaces;
    using Core.Ioc;
    using Core.Services.Migrations;
    using Core.Utilities;
    using Core.Interfaces.Services;
    using Core.Reflection;
    using Core.Services;
    using Unity;

    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            AreaRegistration.RegisterAllAreas();
            System.Web.Http.GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);

            // Start unity
            UnityHelper.InitialiseUnityContainer();

            // Make DB update to latest migration
            Database.SetInitializer(new MigrateDatabaseToLatestVersion<MvcForumContext, Configuration>());

            // Set the rest of the Ioc
            UnityHelper.BuildUnityContainer();

            // Grab the container as we will need to use it
            var unityContainer = UnityHelper.Container;

            // Set Hangfire to use SQL Server and the connection string
            GlobalConfiguration.Configuration.UseSqlServerStorage(ForumConfiguration.Instance.MvcForumContext);

            // Make hangfire use unity container
            GlobalConfiguration.Configuration.UseUnityActivator(unityContainer);

            // Add Hangfire
            // TODO - Do I need this dashboard?
            //app.UseHangfireDashboard();
            app.UseHangfireServer();

            // Get services needed
            var mvcForumContext = unityContainer.Resolve<IMvcForumContext>();
            var badgeService = unityContainer.Resolve<IBadgeService>();
            var loggingService = unityContainer.Resolve<ILoggingService>();
            var assemblyProvider = unityContainer.Resolve<IAssemblyProvider>();

            // Routes
            RouteConfig.RegisterRoutes(RouteTable.Routes);

            // If the same carry on as normal            
            var logFileSize = ForumConfiguration.Instance.LogFileMaxSizeBytes;
            loggingService.Initialise(logFileSize > 100000 ? logFileSize : 100000);
            loggingService.Error("START APP");

            // Find the plugin, pipeline and badge assemblies
            var assemblies = assemblyProvider.GetAssemblies(ForumConfiguration.Instance.PluginSearchLocations).ToList();
            ImplementationManager.SetAssemblies(assemblies);

            // Do the badge processing
            try
            {
                badgeService.SyncBadges(assemblies);
                mvcForumContext.SaveChanges();
            }
            catch (Exception ex)
            {
                loggingService.Error($"Error processing badge classes: {ex.Message}");
            }

            var theme = "Metro";
            var settings = mvcForumContext.Setting.FirstOrDefault();
            if (settings != null)
            {
                theme = settings.Theme;
            }

            // Set the view engine
            ViewEngines.Engines.Clear();
            ViewEngines.Engines.Add(new ForumViewEngine(theme));

            // Initialise the events
            EventManager.Instance.Initialize(loggingService, assemblies);

            // Finally trigger any Cron jobs
            RecurringJob.AddOrUpdate<RecurringJobService>(x => x.SendMarkAsSolutionReminders(), Cron.HourInterval(6), queue: "solutionreminders");            
        }
    }
}